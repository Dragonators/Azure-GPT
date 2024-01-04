﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
                .OrderByDescending(i=> i.latestAt)
                .ToListAsync();//no chatmessages
            return new JsonResult(JsonSerializer.Serialize(Navlinks));
        }
        [HttpGet("GetHisAsync")]
        public async Task<IActionResult> GetHisAsync([FromQuery] string navId)
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
                Model = Models.Gpt_3_5_Turbo_1106,
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
        //无缓存的获取指定长度的聊天记录
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

        private async Task<IEnumerable<HistoryMessage>> CurrentChatHistory(string navId)
        {
            var history = await _context.Navlinks.AsNoTracking()
                .FirstOrDefaultAsync(i => i.navId == navId);
            _context.Entry(history)
                .Collection(b => b.chatMessages)
                .Load();
            return history.chatMessages.OrderBy(i => i.creatAt).ToImmutableArray();
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
                latestAt = DateTime.Now,
                userId = userId,
                navName = "New Chat"
            });
			await _context.SaveChangesAsync();

			return navId;
		}
        [HttpPost("DeleteNavAsync/{navId}")]
        public async Task<int> DeleteNavAsync(string navId)
        {
            //在数据库中删除指定的navlink
            _context.Remove<Navlink>(await _context.Navlinks.FirstOrDefaultAsync(e => e.navId == navId));
            return await _context.SaveChangesAsync();
        }

        [HttpPost("UpdateNavNameAsync")]
        public async Task<int> UpdateNavNameAsync([FromQuery] string navId, [FromQuery]string navName)
        {
            //在数据库中更新指定的navlink名称
            var navlink = await _context.Navlinks.FirstOrDefaultAsync(e => e.navId == navId);
            navlink.navName = navName;
            _context.Update(navlink);
            return await _context.SaveChangesAsync();
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