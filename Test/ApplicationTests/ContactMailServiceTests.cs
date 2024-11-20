using Care.Web.Application.Common.Exceptions;
using Care.Web.Application.Common.Services;
using Care.Web.Domain.Models;
using System.Reflection;

namespace Care.Web.Test.ApplicationTests
{
    [TestClass]
    public class ContactMailServiceTests
    {
        [TestMethod]
        [DynamicData(nameof(MailsData), DynamicDataSourceType.Method, DynamicDataDisplayName = nameof(DynamicDataDisplayName))]
        public void GetContactEmailAddress_ShouldReturnEmail(string testName, IncomingMail incomingMail, string expectedContactMail)
        {
            //Arrange
            var contactMailService = new ContactMailService();

            //Act 
            string actualContactMail = contactMailService.GetContactEmailAddress(incomingMail.HtmlMail, incomingMail.From);

            //Assert
            Assert.IsNotNull(actualContactMail);
            Assert.AreEqual(expectedContactMail, actualContactMail);
        }

        [TestMethod]
        public void GetContactEmailAddress_ShouldThrowException()
        {
            //Arrange
            var contactMailService = new ContactMailService();

            var incomingMail = new IncomingMail()
            {
                From = new MailPerson()
                {
                    Name = Faker.Name.FullName(),
                    Email = "Hello@Norriq.com",
                },

                To = new List<MailPerson>()
                { new MailPerson()
                    {
                        Name = Faker.Name.FullName(),
                        Email = Faker.Internet.Email(),
                    }
                },

                HtmlMail = "<html><body>Dear Customer Support Team,\n\nI trust this message finds you well. " +
                       "I am a dedicated user of your IT system and have always been satisfied with your services. " +
                       "However, I am writing to bring to your attention a critical issue that I've been facing lately.\n\nSince yesterday, " +
                       "I have been unable to log in to my account. Despite multiple attempts and password resets, " +
                       "the login page does not allow me access. This is highly inconvenient, as it hinders my ability to utilize the features I have subscribed to." +
                       "\n\nI kindly request your prompt attention to this matter and appreciate your efforts to resolve it at the earliest convenience." +
                       "\n\nThank you for your time and assistance.\n\nBest regards,\nJohn Doe</body></html>",
                Subject = "Urgent Account Login Issue",
                Date = DateTime.Now
            };

            //Act 
            Action act = () => contactMailService.GetContactEmailAddress(incomingMail.HtmlMail, incomingMail.From);

            //Assert
            Assert.ThrowsException<ContactMailNotFoundException>(act);
        }

        public static IEnumerable<Object[]> MailsData()
        {
            DirectoryInfo mailsDirectory = new(@"TestMails");

            foreach (DirectoryInfo mailFolder in mailsDirectory.GetDirectories("*", new EnumerationOptions() { MatchCasing = MatchCasing.CaseInsensitive, RecurseSubdirectories = false }))
            {
                FileInfo[] files = mailFolder.GetFiles("*", new EnumerationOptions() { MatchCasing = MatchCasing.CaseInsensitive, RecurseSubdirectories = false });

                string body = string.Empty;
                using (StreamReader sr = files.Where(file => file.Name.Equals("body.html", StringComparison.OrdinalIgnoreCase)).Single().OpenText())
                { body = sr.ReadToEnd().Trim(); }

                string fromEmail = string.Empty;
                using (StreamReader sr = files.Where(file => file.Name.Equals("from.txt", StringComparison.OrdinalIgnoreCase)).Single().OpenText())
                { fromEmail = sr.ReadToEnd().Trim(); }

                string subject = string.Empty;
                using (StreamReader sr = files.Where(file => file.Name.Equals("subject.txt", StringComparison.OrdinalIgnoreCase)).Single().OpenText())
                { subject = sr.ReadToEnd().Trim(); }

                string expectedContactMail = string.Empty;
                using (StreamReader sr = files.Where(file => file.Name.Equals("contactMail.txt", StringComparison.OrdinalIgnoreCase)).Single().OpenText())
                { expectedContactMail = sr.ReadToEnd().Trim(); }

                string testName = "Test Case no.: " + mailFolder.Name.Split('.', StringSplitOptions.TrimEntries)[0];

                yield return new object[] { testName, new IncomingMail() { From = new MailPerson() { Email = fromEmail }, Date = DateTime.Now, HtmlMail = body, Subject = subject }, expectedContactMail };
            };
        }

        public static string? DynamicDataDisplayName(MethodInfo methodInfo, object[] data)
        {
            return data.First() as string;
        }
    }
}