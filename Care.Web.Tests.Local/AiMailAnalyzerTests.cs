using Care.Web.Application.Common.Services;
using Care.Web.Application.Strategies;
using Care.Web.Domain.Enums;
using Care.Web.Domain.Models;
using Care.Web.Domain.Models.OpenAi;
using Microsoft.Extensions.Configuration;
using RestSharp;
using System.Reflection;

namespace Care.Web.Tests.Local;

[TestClass]
public class AiMailAnalyzerTests
{
    private static IConfigurationRoot _config;
    private static ChatRequestParameters _chatRequest;
    private static TokenizerData _tokenizerData;

    [ClassInitialize]
    public static void BeforeClass(TestContext tc)
    {
        _config = new ConfigurationBuilder().AddJsonFile("appsettings.Development.json").Build(); // TODO Injection

        _chatRequest = new ChatRequestParameters();
        _config.GetSection("OpenAi:chatRequestParameters").Bind(_chatRequest);

        _tokenizerData = new TokenizerData();
        _config.GetSection("OpenAi:TokenizerData").Bind(_tokenizerData);
    }

    [TestMethod]
    [DynamicData(nameof(MailsData), DynamicDataSourceType.Method, DynamicDataDisplayName = nameof(DynamicDataDisplayName))]
    public async Task AnalyzeMail_ShouldReturnMailCaseAsync(string testName, IncomingMail incomingMail, Severity expectedSeverity, CaseType expectedCaseType)
    {
        // Arrange
        var restClient = new RestClient(new HttpClient { BaseAddress = new Uri(_config["openAi:apiSettings:baseUrl"]!) }).AddDefaultHeader("api-key", _config["openAi:apiSettings:apiKey"]!);
        var contactMailService = new ContactMailService();
        var mailAnalyzer = new AiMailAnalyzer(restClient, contactMailService);
        var chatResponseService = new ChatResponseService(mailAnalyzer);
        var chatRequestService = new ChatRequestService(_chatRequest, _tokenizerData);

        (ChatRequestParameters chatRequestParameters, string fullEmailBody) = chatRequestService.GetChatRequest(incomingMail, CancellationToken.None);

        // Act 
        MailCase? mailCase = await chatResponseService.GetChatReponseAsync(incomingMail, chatRequestParameters, fullEmailBody, CancellationToken.None);

        // Assert
        Assert.IsNotNull(mailCase);

        bool isSeverityMatch = expectedSeverity == mailCase.Severity || Math.Abs((int)(expectedSeverity - mailCase.Severity!)) == 1;

        bool isCaseTypeMatch = expectedCaseType == mailCase.CaseType || ((int)expectedCaseType == 1 && (int)(mailCase?.CaseType ?? 0) == 2) ||
                                                                        ((int)expectedCaseType == 2 && (int)(mailCase?.CaseType ?? 0) == 1);

        string severityMessage = isSeverityMatch ? "" : $"Expected severity of {expectedSeverity} but got {mailCase?.Severity}. ";
        string caseTypeMessage = isCaseTypeMatch ? "" : $"Expected case type of {expectedCaseType} but got {mailCase?.CaseType}";

        Assert.IsTrue(isSeverityMatch && isCaseTypeMatch, severityMessage + caseTypeMessage);
    }

    public static IEnumerable<Object[]> MailsData()
    {
        DirectoryInfo mailsDirectory = new(@"TestMails");

        foreach (DirectoryInfo mailFolder in mailsDirectory.GetDirectories("*", new EnumerationOptions() { MatchCasing = MatchCasing.CaseInsensitive, RecurseSubdirectories = false }))
        {
            FileInfo[] files = mailFolder.GetFiles("*", new EnumerationOptions() { MatchCasing = MatchCasing.CaseInsensitive, RecurseSubdirectories = false });

            string body = string.Empty;
            using (StreamReader sr = files.Where(file => file.Name.Equals("body.html", StringComparison.OrdinalIgnoreCase)).Single().OpenText())
            { body = sr.ReadToEnd().Trim(); }

            string fromEmail = string.Empty;
            using (StreamReader sr = files.Where(file => file.Name.Equals("from.txt", StringComparison.OrdinalIgnoreCase)).Single().OpenText())
            { fromEmail = sr.ReadToEnd().Trim(); }

            string subject = string.Empty;
            using (StreamReader sr = files.Where(file => file.Name.Equals("subject.txt", StringComparison.OrdinalIgnoreCase)).Single().OpenText())
            { subject = sr.ReadToEnd().Trim(); }

            string testName = "Test Case no.: " + mailFolder.Name.Split('.', StringSplitOptions.TrimEntries)[0];

            Severity severity = Enum.Parse<Severity>(mailFolder.Name.Split('.')[1].Split(',', StringSplitOptions.TrimEntries)[0], true);
            CaseType caseType = Enum.Parse<CaseType>(mailFolder.Name.Split('.')[1].Split(',', StringSplitOptions.TrimEntries)[1], true);

            yield return new object[] { testName, new IncomingMail() { From = new MailPerson() { Email = fromEmail }, Date = DateTime.Now, HtmlMail = body, Subject = subject }, severity, caseType };
        };
    }

    public static string? DynamicDataDisplayName(MethodInfo methodInfo, object[] data)
    {
        return data.First() as string;
    }
}
