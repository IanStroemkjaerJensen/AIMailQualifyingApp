using Care.Web.Application.Common.Services.Interfaces;
using Care.Web.Domain.Models;
using Care.Web.Domain.Models.OpenAi;
using HtmlAgilityPack;
using Microsoft.DeepDev;

namespace Care.Web.Application.Common.Services;
public class ChatRequestService : IChatRequestService
{
    private readonly ChatRequestParameters _chatRequestParameters;
    private readonly TokenizerData _tokenizerData;
    private readonly ITokenizer _tokenizer;
    public string FullEmailBody { get; set; }

    public ChatRequestService(ChatRequestParameters chatRequest, TokenizerData tokenizerData)
    {
        _chatRequestParameters = chatRequest;
        _tokenizerData = tokenizerData;
        _tokenizer = TokenizerBuilder.CreateByModelNameAsync(_chatRequestParameters.Model, _tokenizerData.SpecialTokens).Result;
    }

    /// <summary>
    /// Builds the request that the OpenAI chat completion endpoint requires. The values of <see cref="ChatRequestParameters"/> are set in the appsettings json files.
    /// </summary>
    /// <param name="mail"></param>
    /// <param name="systemMessage">Only used for debuging/testing. Overwrites the one in the appsettings.json files.</param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public (ChatRequestParameters chatRequest, string FullEmailBody) GetChatRequest(IncomingMail mail, CancellationToken ct, string? systemMessage = null)
    {
        _chatRequestParameters.Messages[1].Content = GetRequestMessage(mail);

        if (systemMessage != null)
        { _chatRequestParameters.Messages[0].Content = systemMessage; }

        return (_chatRequestParameters, FullEmailBody);
    }

    private string GetRequestMessage(IncomingMail mail)
    {
        string deentitizedBody = GetHTMLBody(mail.HtmlMail);

        string baseMessage = "Subject: {0}, mail body: {1}";

        (bool isExceeded, int count) = IsTokenLimitExceeded(string.Format(baseMessage, mail.Subject, deentitizedBody));

        return isExceeded ? string.Format(baseMessage, mail.Subject, ShortenBody(deentitizedBody, count))
                          : string.Format(baseMessage, mail.Subject, deentitizedBody);
    }

    /// <summary>
    /// Shorten the email body to make it shorter than the maximum allowed context window limit set by the OpenAi chat completion endpoint. In order to keep the first message from the customer, only the latest messages are deleted.
    /// </summary>
    /// <param name="mailBody">The mail body (without html tags and html entities) that should be shortere.</param>
    /// <param name="count">How many tokens the <see cref="mailBody"/> should be shortened by.</param>
    /// <returns></returns>
    private string ShortenBody(string mailBody, int count)
    {
        var bodyWithSpecialTokens = _tokenizerData.SpecialTokens.Keys.ElementAt(1) + mailBody + _tokenizerData.SpecialTokens.Keys.ElementAt(0);

        var encodedBody = _tokenizer.Encode(bodyWithSpecialTokens, new HashSet<string>(_tokenizerData.SpecialTokens.Keys));

        encodedBody.RemoveRange(1, count + _tokenizerData.ExtraTokensSubtracted);

        var decodedBody = _tokenizer.Decode(encodedBody.ToArray()).Trim();

        var withoutStartToken = decodedBody.Remove(0, _tokenizerData.SpecialTokens.Keys.ElementAt(1).Length);
        var res = withoutStartToken.Remove(withoutStartToken.Length + 1 - _tokenizerData.SpecialTokens.Keys.ElementAt(0).Length - 1, _tokenizerData.SpecialTokens.Keys.ElementAt(0).Length);

        return res;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="message">The mail body without the html tags and html entities.</param>
    /// <returns>If the message is longer than the context windows set by the OpenAi's chat completion endpoint and by how much.</returns>
    private (bool isExceeded, int tokensExceeded) IsTokenLimitExceeded(string message)
    {
        // Start and stop tokens that the tokenizer needs.
        var text = _tokenizerData.SpecialTokens.Keys.ElementAt(1) + _chatRequestParameters.Messages[0].Content + message + _tokenizerData.SpecialTokens.Keys.ElementAt(0); // System message tæller med i context window grænsen.

        var encoded = _tokenizer.Encode(text, new HashSet<string>(_tokenizerData.SpecialTokens.Keys));
        int specialTokenCount = 2; // Start and stop tokens that the tokenizer needs, are subtracted here, since they are only temporarily part of the message.
        int tokenCount = encoded.Count - specialTokenCount + _chatRequestParameters.MaxTokens!.Value; // Completion tæller med i context window grænsen. https://help.openai.com/en/articles/4936856-what-are-tokens-and-how-to-count-them

        return (tokenCount > _tokenizerData.ModelsContextWindow, tokenCount - _tokenizerData.ModelsContextWindow);
    }

    /// <summary>
    /// Removes the html tags and html entities from the full email.
    /// </summary>
    /// <param name="mailBody"></param>
    /// <returns></returns>
    private string GetHTMLBody(string mailBody)
    {
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(mailBody);

        var body = htmlDoc.DocumentNode.SelectSingleNode("//body");
        //if (body == null)
        //{ return FullEmailBody = mailBody.Trim(); }

        string deentitizedBody = HtmlEntity.DeEntitize(body.InnerText).Trim();

        FullEmailBody = deentitizedBody;
        return deentitizedBody;
    }
}