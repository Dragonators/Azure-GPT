using System.Collections;
using Microsoft.AspNetCore.Mvc;
using OpenAI.Interfaces;
using OpenAI.ObjectModels;
using OpenAI.ObjectModels.RequestModels;
using OpenAI.ObjectModels.ResponseModels;
using System.Collections.Concurrent;
using System.Text;
using System.Net;
using Microsoft.Identity.Client;
using OpenAi_API.Model;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;

namespace OpenAi_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IOpenAIService _openAiService;
        private readonly ILogger<ChatController> _logger;
        private readonly ChatDbContext _context;
		private static readonly ConcurrentDictionary<string, CancellationTokenSource> TokenSources = new();

        public ChatController(IOpenAIService openAiService, ILogger<ChatController> logger,ChatDbContext context)
        {
            _openAiService = openAiService;
            _logger = logger;
            _context = context;
        }
        [HttpGet("GetNavAsync")]
        public async Task<IActionResult> GetNavAsync([FromQuery] string userId)
        {
            var Navlinks = await _context.Navlinks.AsNoTracking()
                .Where(i=>i.userId==userId)
                .OrderByDescending(i=> i.createAt)
                .ToListAsync();//no chatmessages
            return new JsonResult(JsonSerializer.Serialize(Navlinks));
        }
        [HttpGet("GetHisAsync")]
        public async Task<IActionResult> GetHisAsync([FromQuery] string navId)
        {
            var history= await _context.Navlinks.AsNoTracking()
                .FirstOrDefaultAsync(i => i.navId == navId);
            _context.Entry(history)
                .Collection(b => b.chatMessages)
                .Load();
            history.chatMessages= history.chatMessages.OrderBy(i => i.creatAt).ToList();
            return new JsonResult(JsonSerializer.Serialize(history.chatMessages));
        }
        [HttpPost("ChatAsStreamAsync")]
        public async Task SendMessageAsStreamAsync([FromForm] string userId, [FromForm]string message, [FromForm]string navId)
        {
            var cancellationTokenSource = new CancellationTokenSource();
            if (!TokenSources.TryAdd(userId, cancellationTokenSource))
            {
				TokenSources[userId].Cancel();

				if (!TokenSources.TryUpdate(userId, cancellationTokenSource, TokenSources[userId]))
	            {
		            throw new Exception("Unknown Error");
	            }
            }
            var timenow=DateTime.Now;
            
            var completionResult = _openAiService.ChatCompletion.CreateCompletionAsStream(new ChatCompletionCreateRequest
            {

                Messages = new List<ChatMessage>
                {
                    new(StaticValues.ChatMessageRoles.System, "你是一位有能力的中文助理"),
                    new(StaticValues.ChatMessageRoles.User, message),
                    new(StaticValues.ChatMessageRoles.User, "请为我提供一些参考")
                },
                Model = Models.Gpt_3_5_Turbo_1106,
                Temperature = (float?)0.7,
                MaxTokens = 150//optional
            }, null, cancellationTokenSource.Token);

            Response.Headers.Add("Content-Type", "text/event-stream");
            Response.Headers.Add("Cache-Control", "no-cache");
            //Response.Headers.Add("Connection", "keep-alive");
            var gptMessage = new StringBuilder();
			await foreach (var completion in completionResult)
            {
	            if (cancellationTokenSource.IsCancellationRequested) break;
                if (completion.Successful)
                {
	                gptMessage.Append(completion.Choices.First().Message.Content);
					await Response.WriteAsync(completion.Choices.First().Message.Content ?? "",cancellationTokenSource.Token);
                    await Response.Body.FlushAsync(cancellationTokenSource.Token);
                }
                else
                {
                    if (completion.Error == null)
                    {
                        throw new Exception("Unknown Error");
                    }
                    else
                    {
	                    gptMessage.Append(completion.Error.Message);
						await Response.WriteAsync(completion.Error.Message, cancellationTokenSource.Token);
                        await Response.Body.FlushAsync(cancellationTokenSource.Token);
                    }
                }
            }
            if(!cancellationTokenSource.IsCancellationRequested)
            {
	            Response.Body.Close();
	            await SaveChatRecord(message, navId, StaticValues.ChatMessageRoles.User, timenow);

	            await SaveChatRecord(gptMessage.ToString(), navId, StaticValues.ChatMessageRoles.Assistant,DateTime.Now);
                (await _context.Navlinks.FirstOrDefaultAsync(i=>i.navId==navId)).createAt=DateTime.Now;
                await _context.SaveChangesAsync();
            }
		}
        private async Task<IEnumerable<ChatMessage>> CurrentChatHistory(string navId)
        {
            var history = await _context.Navlinks.AsNoTracking()
                .FirstOrDefaultAsync(i => i.navId == navId);
            _context.Entry(history)
                .Collection(b => b.chatMessages)
                .Load();
            history.chatMessages = history.chatMessages.OrderBy(i => i.creatAt).ToList();
            var chatHistory = new List<ChatMessage>();
            foreach (var item in history.chatMessages)
            {
                chatHistory.Add(new ChatMessage(item.role, item.message));
            }
            return chatHistory;
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
        [HttpPost("CreateNavId/{userId}")]
        public async Task<string> CreateNavId(string userId)
        {
            var navId = Guid.NewGuid().ToString();
            //在数据库中添加新的navlink
            _context.Add(new Navlink
            {
                navId = "gpt"+navId,
                createAt = DateTime.Now,
                userId = userId,
                navName = "New Chat"
            });
			await _context.SaveChangesAsync();

			return navId;
		}
        private async Task SaveChatRecord(string text,string navId,string role,DateTime time)
        {
	        _context.Add(new HistoryMessage
            {
				message=text,
                creatAt = time,
                navId = navId,
                role = role
			});
	        await _context.SaveChangesAsync();
		}

	}
}