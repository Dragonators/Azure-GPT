using IdentityModel;
using IdentityServer;
using IdentityServer.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace IdentityServerHost.Pages.Admin
{
	[IgnoreAntiforgeryToken]//WHY
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

		public void OnGet()
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
					result.Errors.ToList().ForEach(e => ModelState.AddModelError(e.Code, e.Description));
					return Page();
				}
			}
			return Page();
		}
		public async Task<IActionResult> OnPostPwdReset(string id,[Required]string new_pwd)
		{
			Log.Information("heelo "+id+" "+new_pwd);			
			return Page();
		}
		public async Task<IActionResult> OnPostCreate(ApplicationUser user,string pwd,string roll)
        {
            var result = await _userManager.CreateAsync(user,pwd);
            result = await _userManager.AddToRoleAsync(user, roll);
            return RedirectToPage("/Account/Admin/Index");
        }
		public async Task<IActionResult> OnPostValidUsername([FromBody]Jdata data)
		{
			return new JsonResult((await _userManager.FindByNameAsync(data.username)) is null|| (await _userManager.FindByNameAsync(data.username)).Id== data.id);
		}
    }
	public class Jdata
	{
		public string id { get; set; }
		public string username { get; set; }
	}
}
