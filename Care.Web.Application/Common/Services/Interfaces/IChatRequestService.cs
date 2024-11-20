using Care.Web.Domain.Models;
using Care.Web.Domain.Models.OpenAi;

namespace Care.Web.Application.Common.Services.Interfaces;
public interface IChatRequestService
{
    string FullEmailBody { get; set; }

    (ChatRequestParameters chatRequest, string FullEmailBody) GetChatRequest(IncomingMail mail, CancellationToken ct, string? systemMessage = null);
}
