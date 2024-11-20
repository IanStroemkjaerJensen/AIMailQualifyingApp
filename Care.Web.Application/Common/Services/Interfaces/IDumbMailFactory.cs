using Care.Web.Domain.Models;

namespace Care.Web.Application.Common.Services.Interfaces;

public interface IDumbMailFactory
{
    Task<MailCase> CreateDumbMailCase(IncomingMail mail, CancellationToken ct);
}
