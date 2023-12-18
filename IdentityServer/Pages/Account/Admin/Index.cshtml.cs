using IdentityModel;
using IdentityServer.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Security.Claims;

namespace IdentityServerHost.Pages.Admin
{
	[Authorize(Roles = "Administrator")]
	public class IndexModel : PageModel
    {
		public string content { get; private set; }
		public UserModel AllUser { get; set; }
		public UserManager<ApplicationUser> _userManager { get; set; }
		[BindProperty]
		public CreateInputModel Input { get; set; }
		public IndexModel(UserManager<ApplicationUser> userManager)
		{
			_userManager = userManager;
		}

		public async Task OnGet()
        {
            AllUser = new UserModel
            {
                Users = await _userManager.Users.Where(d => !d.tdIsDelete).OrderBy(d => d.UserName).ToListAsync(),

                userManager = _userManager
            };
			//content =HttpContext.User.FindFirstValue(JwtClaimTypes.Role);		
        }
		public async Task<IActionResult> OnPostDelete(string id)
		{
			var user = await _userManager.FindByIdAsync(id);
            user.tdIsDelete = true;
			await _userManager.DeleteAsync(user);
			return RedirectToPage("/Account/Admin/Index");
        }
        public async Task<IActionResult> OnPostUpdate()
        {
			//return RedirectToPage("/Account/Admin/Index");
			if (!ModelState.IsValid)
			{
				// 如果模型状态不合法，返回错误信息
				return BadRequest(ModelState);
			}
			return RedirectToPage("/Account/Admin/Index");
		}
		public async Task<IActionResult> OnPostCreate(ApplicationUser user,string pwd,string roll)
        {
            var result = await _userManager.CreateAsync(user,pwd);
            result = await _userManager.AddToRoleAsync(user, roll);
            return RedirectToPage("/Account/Admin/Index");
        }
    }
}
