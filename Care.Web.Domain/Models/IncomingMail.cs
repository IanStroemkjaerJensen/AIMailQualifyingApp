using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace Care.Web.Domain.Models
{
    public class IncomingMail
    {
        [Required]
        public MailPerson? From { get; set; }

        public List<MailPerson> To { get; init; } = new();

        public List<MailPerson> Cc { get; } = new();

        [Required]
        public string? HtmlMail { get; set; }

        [Required]
        public string? Subject { get; set; }

        [Required]
        public DateTime? Date { get; set; }

        public List<MailAttachment> Attachments { get; } = new();
    }

    public class MailPerson
    {
        public string? Name { get; set; }

        private string _email;
        [Required]
        public string? Email
        {
            set
            {
                // Truncates everything that is not part of the email. Ex. "John Doe <john.doe@domain>" becomes "john.doe@domain".
                string pattern = @"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b";
                Match match = Regex.Match(value!, pattern);
                if (match.Success)
                {
                    var emailAddress = new MailAddress(match.Value); // Throws exception if the email is invalid.
                    _email = emailAddress.Address;
                }
                else
                {
                    throw new FormatException("Invalid email format");
                }
            }
            get => _email;
        }
    }

    public class MailAttachment
    {

        public string? Name { get; set; }

        [Required]
        public byte[]? Content { get; set; }
    }
}
