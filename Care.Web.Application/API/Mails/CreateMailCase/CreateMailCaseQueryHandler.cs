using Care.Web.Application.API.Mails.ChatRequest;
using Care.Web.Application.API.Mails.ChatResponse;
using Care.Web.Application.API.Mails.CreateDumbMailCase;
using Care.Web.Application.Common.Services.Interfaces;
using Care.Web.Common;
using Care.Web.Domain.Models;
using Care.Web.Domain.Models.OpenAi;
using MediatR;

namespace Care.Web.Application.API.Mails.CreateMailCase;

public class CreateMailCaseQueryHandler : IRequestHandler<CreateMailCaseQuery, Result<MailCase?>>
{
    private readonly IMediator _mediator;

    public CreateMailCaseQueryHandler(IMediator mediator)
    {
        this._mediator = mediator;
    }

    public async Task<Result<MailCase?>> Handle(CreateMailCaseQuery request, CancellationToken ct)
    {
        (ChatRequestParameters chatRequest, string FullEmailBody) chatRequest = await _mediator.Send(new GetChatRequestQuery(request.Mail, request.SystemMessage));

        Result<MailCase?> mailCaseResult = await _mediator.Send(new PostChatRequestQuery(request.Mail, chatRequest.chatRequest, chatRequest.FullEmailBody, ct));

        if (mailCaseResult.Failure) // Fallback logic. In case AI failed to classify email.
        {
            mailCaseResult = Result.Ok(await _mediator.Send(new CreateDumbMailCaseQuery(request.Mail)));
        }

        return mailCaseResult;
    }
}
