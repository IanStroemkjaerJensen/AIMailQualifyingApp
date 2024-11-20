using Care.Web.Domain.Models;
using Care.Web.Domain.Models.OpenAi;
using MediatR;

namespace Care.Web.Application.Mails.AnalyzeMail;

public record AnalyzeMailQuery(IncomingMail Mail, ChatRequestParameters ChatRequest, string FullMailBody) : IRequest<MailCase?>;
