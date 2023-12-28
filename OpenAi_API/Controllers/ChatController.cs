using Microsoft.AspNetCore.Mvc;
using OpenAI.Interfaces;
using OpenAI.ObjectModels;
using OpenAI.ObjectModels.RequestModels;
using OpenAI.ObjectModels.ResponseModels;
using System.Collections.Concurrent;
using System.Text;

namespace OpenAi_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IOpenAIService _openAiService;
        private readonly ILogger<ChatController> _logger;
        private static readonly ConcurrentDictionary<string, CancellationTokenSource> TokenSources = new();

        public ChatController(IOpenAIService openAiService, ILogger<ChatController> logger)
        {
            _openAiService = openAiService;
            _logger = logger;
        }

        [HttpGet("TEST/{userId}")]
        public async IAsyncEnumerable<string> Get_(string userId, string message = "Please write an eaasy praising your motherland")
        {
			var cancellationTokenSource = new CancellationTokenSource();
			if (!TokenSources.TryAdd(userId, cancellationTokenSource))
			{
				if (!TokenSources.TryGetValue(userId, out cancellationTokenSource)) throw new Exception("Unknown Error");
			}

			var completionResult = _openAiService.ChatCompletion.CreateCompletionAsStream(new ChatCompletionCreateRequest
			{
				Messages = new List<ChatMessage>
				{
					new(StaticValues.ChatMessageRoles.System, "you are a helpful assistant"),
					new(StaticValues.ChatMessageRoles.User, message),
					new(StaticValues.ChatMessageRoles.User, "please give me some reference")
				},
				Model = Models.Gpt_3_5_Turbo_1106,
				Temperature = (float?)0.7
				//MaxTokens  b = 150//optional
			}, null, cancellationTokenSource.Token);
			await foreach (var completion in completionResult)
			{
				if (completion.Successful)
				{
					yield return completion.Choices.First().Message.Content;
				}
				else
				{
					if (completion.Error == null)
					{
						throw new Exception("Unknown Error");
					}
				}
			}
		}

        [HttpGet("async/{userId}")]
        public IAsyncEnumerable<ChatCompletionCreateResponse> PostMessageAsync(string userId, [FromBody]string message = "Please write an eaasy praising your motherland")
        {
			var cancellationTokenSource = new CancellationTokenSource();
			if (!TokenSources.TryAdd(userId, cancellationTokenSource))
			{
				if (!TokenSources.TryGetValue(userId, out cancellationTokenSource)) throw new Exception("Unknown Error");
			}

			var completionResult = _openAiService.ChatCompletion.CreateCompletionAsStream(new ChatCompletionCreateRequest
			{
				Messages = new List<ChatMessage>
				{
					new(StaticValues.ChatMessageRoles.System, "you are a helpful assistant"),
					new(StaticValues.ChatMessageRoles.User, message),
					new(StaticValues.ChatMessageRoles.User, "please give me some reference")
				},
				Model = Models.Gpt_3_5_Turbo_1106,
				Temperature = (float?)0.7
				//MaxTokens  b = 150//optional
			}, null, cancellationTokenSource.Token);
			return completionResult;
        }

        private async IAsyncEnumerable<string> MessageContent(string userId, string message)
        {
	        var cancellationTokenSource = new CancellationTokenSource();
	        if (!TokenSources.TryAdd(userId, cancellationTokenSource))
	        {
		        if (!TokenSources.TryGetValue(userId, out cancellationTokenSource)) throw new Exception("Unknown Error");
	        }

	        var completionResult = _openAiService.ChatCompletion.CreateCompletionAsStream(new ChatCompletionCreateRequest
	        {
		        Messages = new List<ChatMessage>
		        {
			        new(StaticValues.ChatMessageRoles.System, "you are a helpful assistant"),
			        new(StaticValues.ChatMessageRoles.User, message),
			        new(StaticValues.ChatMessageRoles.User, "please give me some reference")
		        },
		        Model = Models.Gpt_4_1106_preview,
		        Temperature = (float?)0.7
		        //MaxTokens  b = 150//optional
	        }, null, cancellationTokenSource.Token);
            await foreach (var completion in completionResult)
            {
                if (completion.Successful)
                {
                    yield return completion.Choices.First().Message.Content;
                }
                else
                {
                    if (completion.Error == null)
                    {
                        throw new Exception("Unknown Error");
                    }
                }
            }
        }
        [HttpPost("sync/{userId}")]
        public async Task<ActionResult<string>> PostMessageSync(string message, string userId)
        {
            var cancellationTokenSource = new CancellationTokenSource();
            if (!TokenSources.TryAdd(userId, cancellationTokenSource))
            {
                if (!TokenSources.TryGetValue(userId, out cancellationTokenSource)) throw new Exception("Unknown Error");
            }

            var completionResult = await _openAiService.ChatCompletion.CreateCompletion(new ChatCompletionCreateRequest
            {
                Messages = new List<ChatMessage>
                {
                    new(StaticValues.ChatMessageRoles.System, "你是一位中文助理"),
                    new(StaticValues.ChatMessageRoles.User, message),
                    new(StaticValues.ChatMessageRoles.User, "请为我提供一些参考")
                },
                Model = Models.Gpt_3_5_Turbo_1106,
                Temperature = (float?)0.7
                //MaxTokens  b = 150//optional
            }, null, cancellationTokenSource.Token);
            var responseBuilder = new StringBuilder();
            if (completionResult.Successful)
            {
                responseBuilder.AppendLine(completionResult.Choices.First().Message.Content);
            }
            else
            {
                if (completionResult.Error == null)
                {
                    throw new Exception("Unknown Error");
                }

                responseBuilder.AppendLine($"{completionResult.Error.Code}: {completionResult.Error.Message}");
            }
            return responseBuilder.ToString();
        }

        [HttpPost("async")]
        public async Task PostMessageAsync_(string _message)
        {
            var completionResult = _openAiService.ChatCompletion.CreateCompletionAsStream(new ChatCompletionCreateRequest
            {
                Messages = new List<ChatMessage>
                {
                    new(StaticValues.ChatMessageRoles.System, "你是一位中文助理"),
                    new(StaticValues.ChatMessageRoles.User, _message),
                    //new(StaticValues.ChatMessageRoles.System, "The Los Angeles Dodgers won the World Series in 2020."),
                    new(StaticValues.ChatMessageRoles.User, "请为我提供一些参考")
                },
                Model = Models.Gpt_4_1106_preview,
                //MaxTokens = 150//optional
            });

            await foreach (var completion in completionResult)
            {
                if (completion.Successful)
                {
                    Console.Write(completion.Choices.First().Message.Content);
                }
                else
                {
                    if (completion.Error == null)
                    {
                        throw new Exception("Unknown Error");
                    }
                    _logger.Log(LogLevel.Error, $"{completion.Error.Code}: {completion.Error.Message}");
                }
            }
        }

        [HttpPost("CancelOperation/{userId}")]
        public ActionResult CancelOperation(string userId)
        {
            if (TokenSources.ContainsKey(userId))
            {
                TokenSources[userId].Cancel();
                return Ok("Operation cancelled");
            }
            else
            {
                return StatusCode(500, "Error: can't cancel for the user");
            }
        }
        [NonAction]
        public CancellationTokenSource GetCancellationToken(string userId)
        {
            if (TokenSources.ContainsKey(userId))
            {
                return TokenSources[userId];
            }
            else
            {
                throw new Exception("Error: can't find the user");
            }
        }
    }
}