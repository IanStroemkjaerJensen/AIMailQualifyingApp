using Care.Web.Application.Common.Services.Interfaces;
using Care.Web.Domain.Models.OpenAi;
using MediatR;

namespace Care.Web.Application.API.Mails.ChatRequest;

public class GetChatQueryHandler : IRequestHandler<GetChatRequestQuery, (ChatRequestParameters chatRequest, string FullEmailBody)>
{
    private readonly IChatRequestService _chatRequestService;

    public GetChatQueryHandler(IChatRequestService chatRequestService)
    {
        this._chatRequestService = chatRequestService;
    }

    public Task<(ChatRequestParameters chatRequest, string FullEmailBody)> Handle(GetChatRequestQuery request, CancellationToken ct)
    {
        var chatRequest = _chatRequestService.GetChatRequest(request.Mail, ct, request.SystemMessage);

        return Task.FromResult(chatRequest);
    }
}