using Care.Web.Application.Common.Exceptions;
using Care.Web.Application.Common.Services.Interfaces;
using Care.Web.Domain.Models;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace Care.Web.Application.Common.Services;

public class ContactMailService : IContactMailService
{
    /// <summary>
    /// Takes the email from the <see cref="MailPerson.Email"/> otherwise it finds it from the <see cref="IncomingMail.HtmlMail"/> body
    /// </summary>
    /// <param name="body"></param>
    /// <param name="from"></param>
    /// <returns>Valid email</returns>
    /// <exception cref="ContactMailNotFoundException"></exception>
    public string GetContactEmailAddress(string body, MailPerson from)
    {
        if (!IsFromNorriq(from.Email!) && IsValidEmail(from.Email!))
        {

            return from.Email!;
        }
        else
        {
            return GetContactEmailFromHTMLBody(body) ?? throw new ContactMailNotFoundException();
        }
    }

    private static bool IsFromNorriq(string fromMail)
    {
        return fromMail.Contains("@norriq", StringComparison.OrdinalIgnoreCase);
    }


    private static string? GetContactEmailFromHTMLBody(string htmlMail)
    {
        // Pattern for finding the from email addresses in the HTML email.
        string fromPattern = @"(From|Fra|Von):(.*?)(<([a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,})>|&lt;([a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,})&gt;)";
        Regex FromRegex = new(fromPattern, RegexOptions.IgnoreCase);
        Match? lastMatch = FromRegex.Matches(htmlMail).LastOrDefault();

        if (lastMatch == null)
        {
            // Pattern for finding all the email addresses in the HTML email.
            string alternativePattern = @"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}";
            Regex regex = new(alternativePattern, RegexOptions.IgnoreCase);
            lastMatch = regex.Matches(htmlMail).LastOrDefault();
        }

        string? lastMail = lastMatch?.Groups[^1].Value;

        if (lastMail == null || IsFromNorriq(lastMail) || !IsValidEmail(lastMail))
        { return null; }

        return lastMail;
    }

    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        { return false; }

        try
        {
            _ = new MailAddress(email);
            return true;
        }
        catch
        { return false; }
    }
}