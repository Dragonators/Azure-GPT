using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebClient.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AccessTokenController : ControllerBase
    {
        [HttpGet("UserToken")]
        public async Task<string> GetAccessToken()
        {
            var token = await HttpContext.GetUserAccessTokenAsync();
            if (token != null)
            {
                return token;
            }
            return "";
        }
    }
}
