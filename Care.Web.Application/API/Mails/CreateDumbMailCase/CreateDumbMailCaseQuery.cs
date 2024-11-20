using Care.Web.Domain.Models;
using MediatR;

namespace Care.Web.Application.API.Mails.CreateDumbMailCase;

public record CreateDumbMailCaseQuery(IncomingMail Mail) : IRequest<MailCase>;