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
public class ConsistencyTest
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
    public async Task ConsistencyTest_ShouldReturnTheSameSeverityAndCaseTypeEverytime(string testName, IncomingMail incomingMail)
    {
        // Arrange
        var restClient = new RestClient(new HttpClient { BaseAddress = new Uri(_config["openAi:apiSettings:baseUrl"]!) }).AddDefaultHeader("api-key", _config["openAi:apiSettings:apiKey"]!);
        var contactMailService = new ContactMailService();
        var mailAnalyzer = new AiMailAnalyzer(restClient, contactMailService);
        var chatResponseService = new ChatResponseService(mailAnalyzer);
        var chatRequestService = new ChatRequestService(_chatRequest, _tokenizerData);
        (ChatRequestParameters chatRequestParameters, string fullEmailBody) = chatRequestService.GetChatRequest(incomingMail, CancellationToken.None);

        int NumberOfTests = 3;

        // Act 
        List<Severity?> severities = new();
        List<CaseType?> caseTypes = new();

        for (int i = 0; i < NumberOfTests; i++)
        {
            MailCase? mailCase = await chatResponseService.GetChatReponseAsync(incomingMail, chatRequestParameters, fullEmailBody, CancellationToken.None);

            severities.Add(mailCase?.Severity);
            caseTypes.Add(mailCase?.CaseType);
        }

        // Assert
        Assert.IsTrue(
        (severities.Distinct().Count() == 1 && caseTypes.Distinct().Count() == 1),
        (severities.Distinct().Count() != 1
            ? $"During {NumberOfTests} tests, {severities.Distinct().Count()} different severities was returned: {string.Join(", ", severities)}. "
            : "")
        + (caseTypes.Distinct().Count() != 1
            ? $"During {NumberOfTests} tests, {caseTypes.Distinct().Count()} different caseTypes was returned: {string.Join(", ", caseTypes)}"
            : "")
        );
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

            yield return new object[] { testName, new IncomingMail() { From = new MailPerson() { Email = fromEmail }, Date = DateTime.Now, HtmlMail = body, Subject = subject } };
        };
    }

    public static string? DynamicDataDisplayName(MethodInfo methodInfo, object[] data)
    {
        return data.First() as string;
    }
}
