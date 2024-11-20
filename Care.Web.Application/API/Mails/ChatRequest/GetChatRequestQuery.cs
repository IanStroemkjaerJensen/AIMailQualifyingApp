using Care.Web.Domain.Models;
using Care.Web.Domain.Models.OpenAi;
using MediatR;

namespace Care.Web.Application.API.Mails.ChatRequest;
public record GetChatRequestQuery(IncomingMail Mail, string? SystemMessage = null) : IRequest<(ChatRequestParameters chatRequest, string fullEmailBody)>;

