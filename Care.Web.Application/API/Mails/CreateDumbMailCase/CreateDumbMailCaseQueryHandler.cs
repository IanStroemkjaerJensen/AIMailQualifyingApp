using Care.Web.Application.Common.Services.Interfaces;
using Care.Web.Domain.Models;
using MediatR;

namespace Care.Web.Application.API.Mails.CreateDumbMailCase;

public class CreateDumbMailCaseQueryHandler : IRequestHandler<CreateDumbMailCaseQuery, MailCase>
{
    private readonly IDumbMailFactory _mailFactory;

    public CreateDumbMailCaseQueryHandler(IDumbMailFactory mailFactory)
    {
        this._mailFactory = mailFactory;
    }

    public async Task<MailCase> Handle(CreateDumbMailCaseQuery request, CancellationToken ct)
    {
        var mailCase = await _mailFactory.CreateDumbMailCase(request.Mail, ct);

        return mailCase;
    }
}
