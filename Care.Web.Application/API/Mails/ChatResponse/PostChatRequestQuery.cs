using Care.Web.Common;
using Care.Web.Domain.Models;
using Care.Web.Domain.Models.OpenAi;
using MediatR;

namespace Care.Web.Application.API.Mails.ChatResponse
{
    public record PostChatRequestQuery(IncomingMail Mail, ChatRequestParameters ChatRequest, string FullMailBody, CancellationToken Ct) : IRequest<Result<MailCase?>>;
}
