using Care.Web.Common;
using Care.Web.Domain.Models;
using MediatR;

namespace Care.Web.Application.API.Mails.CreateMailCase;
public record CreateMailCaseQuery(IncomingMail Mail, string? SystemMessage = null) : IRequest<Result<MailCase?>>;
