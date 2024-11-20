using Care.Web.Application.Common.Services.Interfaces;
using Care.Web.Common;
using Care.Web.Common.ValueObjects;
using Care.Web.Domain.Models;
using Care.Web.Domain.Models.OpenAi;
using System.Net;

namespace Care.Web.Application.Common.Services
{
    public class ChatResponseService : IPostChatRequestService
    {

        private readonly IAiMailAnalyzer _analyzer;

        public ChatResponseService(IAiMailAnalyzer analyzer)
        {
            _analyzer = analyzer;
        }

        /// <summary>
        /// Logic that if OpenAI's API returns the specified errors <see cref="HttpStatusCode.TooManyRequest"/>, <see cref="HttpStatusCode.InternalServerError"/> or
        /// <see cref="HttpStatusCode.ServiceUnavailable"/> it will retry after an exponentially increasing time
        /// </summary>
        /// <param name="mail"></param>
        /// <param name="chatRequest"></param>
        /// <param name="fullMailBody"></param>
        /// <param name="ct"></param>
        /// <param name="retries"></param>
        /// <param name="initialWaitTime"></param>
        /// <param name="waitTimeMultiplier">Multiplier with a default value of 3. initialWaitTime gets increased by this number for every retry</param>
        /// <returns></returns>

        public async Task<Result<MailCase?>> GetChatReponseAsync(IncomingMail mail, ChatRequestParameters chatRequest, string fullMailBody, CancellationToken ct, int retries = 4, int initialWaitTime = 10000, int waitTimeMultiplier = 3)
        {
            Result<MailCase?> result = Result.Fail<MailCase?>(new Error("500", "Oh no, this should never happen", 500));

            for (int i = 0; i < retries; i++)
            {
                result = await _analyzer.AnalyzeMailAsync(mail, chatRequest, fullMailBody, ct);
                if (result.Failure && (result.Error!.StatusCode == (int)HttpStatusCode.TooManyRequests ||
                                            result.Error.StatusCode == (int)HttpStatusCode.InternalServerError ||
                                            result.Error.StatusCode == (int)HttpStatusCode.ServiceUnavailable)) // https://platform.openai.com/docs/guides/error-codes/api-errors
                {
                    await Task.Delay(initialWaitTime, ct);
                    initialWaitTime *= waitTimeMultiplier;
                }
                else
                { break; }
            }

            return result;
        }
    }
}
