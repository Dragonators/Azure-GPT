using IdentityServer.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IdentityServer.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize(Roles = "Administrator")]
	public class AdminsController : ControllerBase
	{
		private readonly UserManager<ApplicationUser> _userManager;
		public AdminsController(UserManager<ApplicationUser> usermanager) 
		{
			_userManager = usermanager;
		}
		[HttpGet]
		public async Task<ActionResult<IEnumerable<ApplicationUser>>> OnGet()
		{
			var admins = await _userManager.Users
				.Where(d => !d.tdIsDelete)
				.OrderBy(d => d.UserName)
				.ToListAsync();
			return admins;
		}
	}
}
