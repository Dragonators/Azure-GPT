using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using OpenAI.Interfaces;
using OpenAI.ObjectModels;
using OpenAI.ObjectModels.RequestModels;
using OpenAi_API.Model;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Text;
using System.Text.Json;

namespace OpenAi_API.Controllers
{
	[ApiController]
    [Route("[controller]")]
    [Authorize(Policy = "ChatApi")]
    public class ChatController : ControllerBase
    {
        private readonly IOpenAIService _openAiService;
        private readonly ILogger<ChatController> _logger;
        private readonly ChatDbContext _context;
        private readonly IMemoryCache _memoryCache;
		private static readonly ConcurrentDictionary<string, CancellationTokenSource> TokenSources = new();

        public ChatController(IOpenAIService openAiService, ILogger<ChatController> logger,ChatDbContext context,IMemoryCache memoryCache)
        {
            _openAiService = openAiService;
            _logger = logger;
            _context = context;
            _memoryCache = memoryCache;
        }
        [HttpGet("GetNavAsync")]
        public async Task<IActionResult> GetNavAsync([FromQuery] string userId)
        {
            var Navlinks=await _context.Navlinks.AsNoTracking()
                    .Where(i=>i.userId==userId)
                    .OrderByDescending(i=> i.latestAt)
                    .ToListAsync();
            return new JsonResult(JsonSerializer.Serialize(Navlinks));
        }
        [HttpGet("GetHisAsync")]
        public async Task<IActionResult> GetHisAsync([FromQuery] string navId)
        {
            return new JsonResult(JsonSerializer.Serialize(await CurrentChatHistory(navId)));
        }
        [HttpGet("GetHisAsStreamAsync")]
        public async Task<IActionResult> GetHisAsStreamAsync([FromQuery] string navId)
        {
            return new JsonResult(JsonSerializer.Serialize(await CurrentChatHistory(navId)));
        }
        [HttpPost("ChatAsStreamAsync")]
        public async Task SendMessageAsStreamAsync([FromForm] string userId, [FromForm]string message, [FromForm]string navId, [FromForm] string prompt="")
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
            var chatContext = new List<ChatMessage>();
            bool noerror = true;

            //添加Prompt
            chatContext.Add(new(StaticValues.ChatMessageRoles.System, Prompts.PromptsDict["Chinese"]));
            //添加历史上下文
            chatContext.AddRange(await CreatechatContext(navId));
            //添加用户输入
            chatContext.Add(new(StaticValues.ChatMessageRoles.User, message));

            var completionResult = _openAiService.ChatCompletion.CreateCompletionAsStream(new ChatCompletionCreateRequest
            {

                Messages = chatContext,
                Model = Models.Gpt_4_1106_preview,
                Temperature = (float?)0.7,
                //MaxTokens = 20//optional
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
                        Response.Body.Close();
                        throw new Exception("Unknown Error");
                    }
                    else
                    {
	                    noerror=false;
                        gptMessage.Append(completion.Error.Message);
						await Response.WriteAsync(completion.Error.Message, cancellationTokenSource.Token);
                        await Response.Body.FlushAsync(cancellationTokenSource.Token);
                    }
                }
            }
            if(!cancellationTokenSource.IsCancellationRequested && noerror)
            {
	            Response.Body.Close();
	            await SaveChatRecord(message, navId, StaticValues.ChatMessageRoles.User, timenow);

	            await SaveChatRecord(gptMessage.ToString(), navId, StaticValues.ChatMessageRoles.Assistant,DateTime.Now);
                (await _context.Navlinks.FirstOrDefaultAsync(i=>i.navId==navId)).latestAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }
		}
        //有缓存的获取指定长度的会话上下文
        private async Task<List<ChatMessage>> CreatechatContext(string navId,int length=int.MaxValue)
        {
            var chatContext = new List<ChatMessage>();
            var chatHistory = (ImmutableArray<HistoryMessage>)await CurrentChatHistory(navId);
            for (int i = chatHistory.Count()/2 < length ? 0: chatHistory.Count() - length * 2; i < chatHistory.Count(); i++)
            {
                chatContext.Add(new ChatMessage(chatHistory[i].role, chatHistory[i].message));
            }
            return chatContext;
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
        [HttpPost("CreateNavIdAsync/{userId}")]
        public async Task<string> CreateNavIdAsync(string userId)
        {
            var navId = Guid.NewGuid().ToString();
            //在数据库中添加新的navlink
            _context.Add(new Navlink
            {
                navId = "gpt"+navId,
                latestAt = DateTime.Now,
                userId = userId,
                navName = "New Chat"
            });
			await _context.SaveChangesAsync();

			return navId;
		}
        [HttpDelete("DeleteNavAsync/{navId}")]
        public async Task<int> DeleteNavAsync(string navId)
        {
            //在数据库中删除指定的navlink
            _context.Remove<Navlink>(await _context.Navlinks.FirstOrDefaultAsync(e => e.navId == navId));
            //删除对应缓存
            _memoryCache.Remove(navId);
            return await _context.SaveChangesAsync();
        }

        [HttpPut("UpdateNavNameAsync")]
        public async Task<int> UpdateNavNameAsync([FromForm] string navId, [FromForm]string navName)
        {
            //在数据库中更新指定的navlink名称
            var navlink = await _context.Navlinks.FirstOrDefaultAsync(e => e.navId == navId);
            navlink.navName = navName;
            _context.Update(navlink);
            return await _context.SaveChangesAsync();
        }
        private async Task<IEnumerable<HistoryMessage>> CurrentChatHistory(string navId)
        {
            //如果缓存中没有聊天记录则从数据库中获取
            var history=await _memoryCache.GetOrCreateAsync<Navlink>(navId, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(60);
                entry.SlidingExpiration = TimeSpan.FromMinutes(10);
                var _history = await _context.Navlinks.AsNoTracking()
                    .FirstOrDefaultAsync(i => i.navId == navId);
                if (_history == null) return null;
                _context.Entry(_history)
                    .Collection(b => b.chatMessages)
                    .Load();
                return _history;
            });
            if (history is null) return null;
            return history.chatMessages.OrderBy(i => i.creatAt).ToImmutableArray();
        }
        private async Task SaveChatRecord(string text,string navId,string role,DateTime time)
        {
            //保存聊天记录到数据库
            var message = new HistoryMessage
            {
                message = text,
                creatAt = time,
                navId = navId,
                role = role
            };
            _context.Add(message);
            await _context.SaveChangesAsync();

            //更新缓存
            Navlink navlink;
            if(_memoryCache.TryGetValue(navId, out navlink))
            {
                navlink.chatMessages.Add(message);
                _memoryCache.Set(navId, navlink);
            }
		}

	}
}