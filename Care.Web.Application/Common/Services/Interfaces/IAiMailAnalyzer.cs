using Care.Web.Common;
using Care.Web.Domain.Models;
using Care.Web.Domain.Models.OpenAi;

namespace Care.Web.Application.Common.Services.Interfaces;

public interface IAiMailAnalyzer
{
    Task<Result<MailCase?>> AnalyzeMailAsync(IncomingMail mail, ChatRequestParameters chatRequest, string fullMailBody, CancellationToken ct);

}
