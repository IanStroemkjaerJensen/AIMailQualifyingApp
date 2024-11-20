using Care.Web.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Care.Web.Domain.Models
{
    public class MailCase
    {
        [Required]
        public DateTime? ReceievedDate { get; set; }
        public string Title { get; set; }
        public string ContactEmail { get; set; }
        public string Description { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public CaseType? CaseType { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Severity? Severity { get; set; }

        public bool IsAiGenerated { get; set; }
    }
}
