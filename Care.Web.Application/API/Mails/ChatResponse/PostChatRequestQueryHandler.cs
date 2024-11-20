using Care.Web.Application.Common.Services.Interfaces;
using Care.Web.Common;
using Care.Web.Domain.Models;
using MediatR;

namespace Care.Web.Application.API.Mails.ChatResponse
{
    public class PostChatRequestQueryHandler : IRequestHandler<PostChatRequestQuery, Result<MailCase?>>
    {
        private readonly IPostChatRequestService _chatResponseService;

        public PostChatRequestQueryHandler(IPostChatRequestService chatResponseService)
        {
            this._chatResponseService = chatResponseService;
        }

        public async Task<Result<MailCase?>> Handle(PostChatRequestQuery request, CancellationToken ct)
        {
            var chatResponse = await _chatResponseService.GetChatReponseAsync(request.Mail, request.ChatRequest, request.FullMailBody, ct);

            return chatResponse;
        }
    }
}
