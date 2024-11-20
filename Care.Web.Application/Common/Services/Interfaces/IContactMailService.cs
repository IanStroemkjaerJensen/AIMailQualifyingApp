using Care.Web.Domain.Models;

namespace Care.Web.Application.Common.Services.Interfaces;
public interface IContactMailService
{
    string GetContactEmailAddress(string body, MailPerson from);
}
