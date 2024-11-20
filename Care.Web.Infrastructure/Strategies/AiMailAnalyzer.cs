using Care.Web.Application.Common.Services.Interfaces;
using Care.Web.Common;
using Care.Web.Common.ValueObjects;
using Care.Web.Domain.Enums;
using Care.Web.Domain.Models;
using Care.Web.Domain.Models.OpenAi;
using RestSharp;
using System.Text.Json;

namespace Care.Web.Application.Strategies;
public class AiMailAnalyzer : IAiMailAnalyzer
{
    private readonly IRestClient _client;
    private readonly IContactMailService _contactMailService;

    public AiMailAnalyzer(IRestClient client, IContactMailService contactMailService)
    {
        this._client = client;
        _contactMailService = contactMailService;
    }

    /// <summary>
    /// Uses OpenAi ChatGPT chat completion endpoint to categorize the <see cref="IncomingMail"/> and return a <see cref="MailCase"/>.
    /// </summary>
    /// <param name="mail">Email to categorize</param>
    /// <param name="chatRequest"></param>
    /// <param name="fullMailBody">Email body (not truncated to be within the context window) without the html tags and entities. Used for the <see cref="MailCase.description"/> on the returned <see cref="MailCase"/>.</param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task<Result<MailCase?>> AnalyzeMailAsync(IncomingMail mail, ChatRequestParameters chatRequest, string fullMailBody, CancellationToken ct)
    {
        var request = new RestRequest().AddJsonBody(chatRequest);

        RestResponse? response = await _client.PostAsync(request, ct);

        if (response == null)
        {
            return Result.Fail<MailCase?>(new Error("500", "The OpenAI API response is null", 500));
        }

        if (String.IsNullOrEmpty(response!.Content))
        { return Result.Fail<MailCase?>(new Error("204", "The OpenAI API responses content is null", 204)); }

        ChatResponse? chatResponse = JsonSerializer.Deserialize<ChatResponse>(response.Content);

        if (chatResponse == null)
        { return Result.Fail<MailCase?>(new Error("500", "Could not deserialize OpenAI API returned content", 500)); }

        MailCase? mailCase = JsonSerializer.Deserialize<MailCase>(chatResponse.Choices[0].Message.Content, new JsonSerializerOptions { PropertyNameCaseInsensitive = false });

        if (mailCase != null)
        {
            mailCase.ReceievedDate = mail.Date;
            mailCase.Title = mail.Subject!;
            mailCase.ContactEmail = _contactMailService.GetContactEmailAddress(mail.HtmlMail!, mail.From!);
            mailCase.Description = fullMailBody;

            mailCase.IsAiGenerated = true;

            if (mailCase.CaseType == CaseType.Request)
            { mailCase.Severity = Severity.None; }
        }
        else
        { return new Result<MailCase?>(null, false, new Error("500", "Could not deserialize the OpenAI API message due to incorrect format", 500)); }

        return Result.Ok(mailCase);
    }
}
