using Microsoft.AspNetCore.Mvc.RazorPages;
using OpenAI.ObjectModels.ResponseModels;
using System;
using System.Security.Claims;
using Newtonsoft.Json;
using System.IO;

namespace WebClient.Pages
{
	public class ChatGPTModel : PageModel
	{
		public Claim? SubClaim { get; set; }
		private readonly IHttpClientFactory _factory;
		private JsonSerializer? _serializer;
		private JsonSerializerSettings? _jsonSerializerSettings;

		public ChatGPTModel(IHttpClientFactory factory)
		{
			_factory = factory;
		}

		public void OnGet()
		{
			SubClaim = User.Claims.FirstOrDefault(c => c.Type == "sub");
		}

		private async Task OnPostTestMessage()
        {
			// initialize the serializer
			this._jsonSerializerSettings = new();
			this._serializer = JsonSerializer.Create(this._jsonSerializerSettings);
			SubClaim = User.Claims.FirstOrDefault(c => c.Type == "sub");

			Console.WriteLine("-------------------------------Begin--------------------------------------", ConsoleColor.DarkRed);

			using var client = _factory.CreateClient("Chat_client"); 
			await using Stream responseStream = await client.GetStreamAsync($"Chat/async/{SubClaim.Value}");
			// set up the stream readers
			using StreamReader sr = new StreamReader(responseStream);
			await using JsonReader jsonReader = new JsonTextReader(sr);

			// move to the start of the array and read the stream
			await jsonReader.ReadAsync().ConfigureAwait(false);

			// walk the collection of objects to the end of the collection
			while (await jsonReader.ReadAsync().ConfigureAwait(false) &&
			       jsonReader.TokenType != JsonToken.EndArray)
			{
				ChatCompletionCreateResponse? response = this._serializer!.Deserialize<ChatCompletionCreateResponse>(jsonReader);
				Process(response!);
			}
			Console.WriteLine("-------------------------------END--------------------------------------", ConsoleColor.DarkRed);
		}
		private void Process(ChatCompletionCreateResponse? completion)
		{
			// process and store the data model
			if (completion.Successful)
			{
				Console.Write(completion.Choices.First().Message.Content);
				ViewData["Message"] += completion.Choices.First().Message.Content;
			}
			else
			{
				if (completion.Error == null)
				{
					throw new Exception("Unknown Error");
				}
				else
				{
					Console.Write(completion.Error.Message);
				}
			}
		}
	}
}