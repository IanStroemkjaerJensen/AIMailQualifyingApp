using System.Text.Json.Serialization;

namespace Care.Web.Domain.Models.OpenAi
{
    public class Message
    {
        /// <summary>
        /// The role of the messages author, in our case system or user
        /// </summary>
        [JsonPropertyName("role")]
        public string Role { get; set; }

        /// <summary>
        /// Content: The contents of the system message or user message (email body to be analyzed)
        /// </summary>
        [JsonPropertyName("content")]
        public string Content { get; set; }
    }
}
