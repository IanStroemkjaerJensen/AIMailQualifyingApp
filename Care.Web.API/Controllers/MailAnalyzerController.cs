using Care.Web.Application.API.Mails.ChatRequest;
using Care.Web.Application.API.Mails.CreateDumbMailCase;
using Care.Web.Application.API.Mails.CreateMailCase;
using Care.Web.Common;
using Care.Web.Domain.Models;
using Care.Web.Domain.Models.OpenAi;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Care.Web.API.Controllers;

[Route("Api/v1/[Controller]")]
[ApiController]
public class MailController : Controller
{
    private readonly IMediator _mediator;
    private readonly ILogger<MailController> _logger;

    public MailController(IMediator mediator, ILogger<MailController> logger)
    {
        this._mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// Might take a few minutes, in case the rate limit is exceeded or any other problems occure that triggers the retry logic.
    /// </remarks>
    /// <param name="mail"></param>
    /// <returns><see cref="MailCase"/> analyzed by an artificiel intelligens, if possible, otherwise a <see cref="MailCase"/> without <see cref="Severity" and/> <see cref="CaseType"/> being set.</returns>
    // POST: MailController/AnalyzeMailAsync
    [HttpPost("AnalyzeMail")]
    public async Task<ActionResult<MailCase>> AnalyzeMail([FromBody] IncomingMail mail)
    {
        try
        {
            Result<MailCase?> mailCaseResult = await _mediator.Send(new CreateMailCaseQuery(mail));

            if (mailCaseResult.Success)
            {
                _logger.LogInformation("MailCase created successfully.");
                return Ok(mailCaseResult.Value);
            }
            else
            {
                _logger.LogError("Status Code: {code}; Message: {message}", new { mailCaseResult?.Error?.StatusCode, mailCaseResult?.Error?.Message, mail });
                return new ContentResult
                {
                    StatusCode = mailCaseResult?.Error?.StatusCode,
                    Content = $"Status Code: {mailCaseResult?.Error?.StatusCode}; Message: {mailCaseResult?.Error?.Message}",
                    ContentType = "text/plain",
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, message: ex.Message, args: mail);
            return StatusCode(500);
        }
    }

    /// <summary>
    /// Does the same as <see cref="AnalyzeMail"/> but with option to input own system message
    /// </summary>
    /// <remarks>
    /// Might take a few minutes, in case the rate limit is exceeded or any other problems occure that triggers the retry logic.
    /// </remarks>
    /// <param name="mail"></param>
    /// <param name="systemMessage">A custom system message that overwrites the one in appsettings.json. Only used for debuging/testing. </param>
    /// <returns><see cref="MailCase"/> analyzed by an artificiel intelligens, if possible, otherwise a <see cref="MailCase"/> without <see cref="Severity" and/> <see cref="CaseType"/> being set.</returns>
    // POST: MailController/AnalyzeMailDebug
    [HttpPost("AnalyzeMailDebug")]
    public async Task<ActionResult<MailCase>> AnalyzeMailDebug([FromBody] IncomingMail mail, string? systemMessage)
    {
        try
        {
            var mailCaseResult = await _mediator.Send(new CreateMailCaseQuery(mail, systemMessage));
            if (mailCaseResult.Success)
            {
                _logger.LogInformation("MailCase created successfully, with a custom system message.");
                return Ok(mailCaseResult.Value);
            }
            else
            {
                _logger.LogError("Status Code: {code}; Message: {message}", new { mail, mailCaseResult?.Error?.StatusCode, mailCaseResult?.Error?.Message });
                return new ContentResult
                {
                    StatusCode = mailCaseResult?.Error?.StatusCode,
                    Content = $"Status Code: {mailCaseResult?.Error?.StatusCode}; Message: {mailCaseResult?.Error?.Message}",
                    ContentType = "text/plain",
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, message: ex.Message, args: mail);
            return StatusCode(500);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="mail"></param>
    /// <returns><see cref="MailCase"/> not created by any artificiel intelligens, so it takes its values from incoming mail, but there is no severity or casetype set</returns>
    // POST: MailController/CreateDumbMailcase
    [HttpPost("CreateDumbMailcase")]
    public async Task<ActionResult<MailCase>> CreateDumbMailcase([FromBody] IncomingMail mail)
    {
        try
        {
            var mailCase = await _mediator.Send(new CreateDumbMailCaseQuery(mail));
            _logger.LogInformation("MailCase created successfully, with artificiel intelligens.");
            return mailCase;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, message: ex.Message, args: mail);
            return StatusCode(500);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="mail"></param>
    /// <returns>The request OpenAi's chat completion endpoint would receive.</returns>
    // POST: MailController/GetChatRequest
    [HttpPost("GetChatRequest")]
    public async Task<ActionResult<ChatRequestParameters>> GetChatRequest([FromBody] IncomingMail mail)
    {
        try
        {
            var chatRequest = await _mediator.Send(new GetChatRequestQuery(mail));

            return chatRequest.chatRequest;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, message: ex.Message, args: mail);
            return StatusCode(500);
        }
    }
}

