using Care.Web.Application.API.Mails.ChatResponse;
using Care.Web.Domain.Models;
using MediatR;

namespace Care.Web.Application.Mails.AnalyzeMail;
public class AnalyzeMailQueryHandler : IRequestHandler<AnalyzeMailQuery, MailCase?>
{
    private readonly IMediator _mediator;

    public AnalyzeMailQueryHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<MailCase?> Handle(AnalyzeMailQuery request, CancellationToken ct)
    {
        var analyzedMail = await _mediator.Send(new PostChatRequestQuery(request.Mail, request.ChatRequest, request.FullMailBody, ct));

        return analyzedMail;
    }
}
