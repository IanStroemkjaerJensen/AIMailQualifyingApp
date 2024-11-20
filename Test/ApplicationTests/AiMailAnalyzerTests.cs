using Care.Web.Application.Common.Services;
using Care.Web.Application.Strategies;
using Care.Web.Domain.Enums;
using Care.Web.Domain.Models;
using Care.Web.Domain.Models.OpenAi;
using Microsoft.Extensions.Configuration;
using RestSharp;
using RichardSzalay.MockHttp;
using System.Text.Json;

namespace Care.Web.Test.ApplicationTests
{
    [TestClass]
    public class AiMailAnalyzerTests
    {
        private static IConfigurationRoot _config;
        private static ChatRequestParameters _chatRequest;
        private static TokenizerData _tokenizerData;

        [ClassInitialize]
        public static void BeforeClass(TestContext tc)
        {
            _config = new ConfigurationBuilder().AddJsonFile("appsettings.Development.json").Build(); // TODO Injection

            _chatRequest = new ChatRequestParameters();
            _config.GetSection("OpenAi:chatRequestParameters").Bind(_chatRequest);

            _tokenizerData = new TokenizerData();
            _config.GetSection("OpenAi:TokenizerData").Bind(_tokenizerData);
        }

        [TestMethod]
        public async Task AnalyzeMail_ShouldReturnMailCaseFromMockApiAsync()
        {
            //Arrange
            Severity expectedSeverity = Severity.Significantly;
            CaseType expectedCaseType = CaseType.Incident;

            ChatResponse chatResponse = new()
            {
                Id = "chatcmpl-8OQb43t3QsT6CgU2vcHjk8A7zajz3",
                Object = "chat.completion",
                Created = DateTimeOffset.FromUnixTimeSeconds(1700832802).DateTime,
                Model = "gpt-35-turbo",
                Choices = new Choice[]
                {
                    new Choice()
                        {
                            Index = 0,
                            FinishReason = FinishReason.Length,
                            Message = new Message()
                            {
                                Role = "assistant",
                                Content = "{\"Severity\":\" " + expectedSeverity + "\",\"CaseType\":\"" + expectedCaseType + "\"}"
                            }
                        }
                },

                Usage = new Usage()
                {
                    PromptTokens = 371,
                    CompletionTokens = 19,
                    TotalTokens = 390
                }
            };

            var restClient = MockRestClientAsync(JsonSerializer.Serialize(chatResponse));

            var contactMailService = new ContactMailService();

            var mailAnalyzer = new AiMailAnalyzer(restClient, contactMailService);

            var chatResponseService = new ChatResponseService(mailAnalyzer);

            var chatRequestService = new ChatRequestService(_chatRequest, _tokenizerData);

            var incomingMail = new IncomingMail()
            {
                From = new MailPerson()
                {
                    Name = Faker.Name.FullName(),
                    Email = Faker.Internet.Email(),
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

            (ChatRequestParameters ChatRequestParameters, string FullEmailBody) = chatRequestService.GetChatRequest(incomingMail, CancellationToken.None);

            //Act 
            MailCase? mailCase = await chatResponseService.GetChatReponseAsync(incomingMail, ChatRequestParameters, FullEmailBody, CancellationToken.None);

            //Assert
            Assert.IsNotNull(mailCase);
            Assert.AreEqual(expectedSeverity, mailCase.Severity);
            Assert.AreEqual(expectedCaseType, mailCase.CaseType);
        }

        public static IRestClient MockRestClientAsync(string json)
        {
            var mockHttp = new MockHttpMessageHandler();

            mockHttp.When("*")
                    .Respond("application/json", json);

            var client = new RestClient(new RestClientOptions("https://Test") { ConfigureMessageHandler = _ => mockHttp });

            return client;
        }
    }
}