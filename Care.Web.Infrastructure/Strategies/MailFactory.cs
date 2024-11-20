using Care.Web.Application.Common.Services.Interfaces;
using Care.Web.Domain.Models;

namespace Care.Web.Infrastructure.Strategies;
internal class MailFactory : IDumbMailFactory
{
    private readonly IContactMailService _contactMailService;

    public MailFactory(IContactMailService contactMailService) => _contactMailService = contactMailService;

    public IContactMailService ContactMailService { get; }

    /// <summary>
    /// Creates a <see cref="MailCase"/> from an <see cref="IncomingMail"/>, without any AI processing.
    /// </summary>
    /// <param name="mail"></param>
    /// <param name="ct"></param>
    /// <returns><see cref="MailCase"/> where the <see cref=""/></returns>
    public Task<MailCase> CreateDumbMailCase(IncomingMail mail, CancellationToken ct)
    {
        return Task.Run(() =>
        {
            return new MailCase()
            {
                ReceievedDate = mail.Date,
                Title = mail.Subject,
                ContactEmail = _contactMailService.GetContactEmailAddress(mail.HtmlMail, mail.From),
                Description = mail.HtmlMail,
                CaseType = null,
                Severity = null,

                IsAiGenerated = false
            };
        });
    }
}
