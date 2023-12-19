using IdentityModel;
using IdentityServer;
using IdentityServer.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Security.Claims;
using System.Text;

namespace IdentityServerHost.Pages.Admin
{
	[Authorize(Roles = "Administrator")]
	public class IndexModel : PageModel
    {
		//public UserModel AllUser { get; set; }
		public UserManager<ApplicationUser> _userManager { get; set; }
		[BindProperty]
		public CreateInputModel Input { get; set; }
		public IndexModel(UserManager<ApplicationUser> userManager)
		{
			_userManager = userManager;
		}

		public async Task OnGet()
        { 
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
			if (ModelState.IsValid)
			{
				var user = await _userManager.FindByIdAsync(Input.Id);
				Config.MapUser(Input, ref user);
				var result = await _userManager.UpdateAsync(user);
				if(result.Succeeded)
				{	
					return RedirectToPage("/Account/Admin/Index");
				}
				else
				{
					return Page();
				}
			}
			return Page();
		}
		public async Task<IActionResult> OnPostCreate(ApplicationUser user,string pwd,string roll)
        {
            var result = await _userManager.CreateAsync(user,pwd);
            result = await _userManager.AddToRoleAsync(user, roll);
            return RedirectToPage("/Account/Admin/Index");
        }
    }
}
