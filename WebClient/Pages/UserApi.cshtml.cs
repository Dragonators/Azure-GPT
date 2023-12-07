using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace WebClient.Pages
{
	public class UserApiModel : PageModel
	{
		private readonly ILogger<UserApiModel> _logger;
        IHttpClientFactory _factory;
        public string content { get; private set; }
        public UserApiModel(ILogger<UserApiModel> logger, IHttpClientFactory factory)
		{
			_logger = logger;
            _factory = factory;
        }

		public async Task OnGet()
		{
            await CallApi();
        }
        public async Task CallApi()
        {
            using (var client = _factory.CreateClient("user_client"))
            {
                //content = JsonSerializer.Serialize(JsonDocument.Parse(await client.GetStringAsync("WeatherForecast")), new JsonSerializerOptions { WriteIndented = true });
                content = JsonSerializer.Serialize(JsonDocument.Parse(await client.GetStringAsync("WeatherForecast/1")), new JsonSerializerOptions { WriteIndented = true });
            }
        }
    }

}
