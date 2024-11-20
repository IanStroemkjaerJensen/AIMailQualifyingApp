using Care.Web.Common;
using Care.Web.Domain.Models;
using Care.Web.Domain.Models.OpenAi;

namespace Care.Web.Application.Common.Services.Interfaces;

public interface IPostChatRequestService
{
    Task<Result<MailCase?>> GetChatReponseAsync(IncomingMail mail, ChatRequestParameters chatRequest, string fullMailBody, CancellationToken ct, int retries = 4, int initialWaitTime = 10000, int waitTimeMultiplier = 3);
}