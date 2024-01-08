using IdentityServer;
using IdentityServer.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Serilog;
using System.ComponentModel.DataAnnotations;

namespace IdentityServerHost.Pages.Admin
{
	[Authorize(Roles = "Administrator")]
	public class IndexModel : PageModel
    {
		public UserManager<ApplicationUser> _userManager { get; set; }
		public RoleManager<IdentityRole> _roleManager { get; set; }
		[BindProperty]
		public CreateInputModel Input { get; set; }
		public IndexModel(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
		{
			_userManager = userManager;
			_roleManager = roleManager;
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
			ModelState.MarkAllFieldsAsSkipped();
			if (TryValidateModel(Input, nameof(Input)))
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
		public async Task<IActionResult> OnPostPwdReset(string id_, [Required] string old_pwd, [Required]string new_pwd)
		{
			ModelState.MarkAllFieldsAsSkipped();
			if (old_pwd is null)
			{
				ModelState.AddModelError(string.Empty, "Please provide correct old pwd.");
				return Page();
			}
			if (new_pwd is null)
			{
				ModelState.AddModelError(string.Empty, "Please provide correct new pwd.");
				return Page();
			}
			var result = await _userManager.ChangePasswordAsync(await _userManager.FindByIdAsync(id_),old_pwd,new_pwd);
			if (result.Succeeded)
			{
				return RedirectToPage("/Account/Admin/Index");
			}
			else
			{
				result.Errors.ToList().ForEach(e => ModelState.AddModelError(e.Code, e.Description));
				return Page();
			}
		}
		public async Task<IActionResult> OnPostRoleUpdate(string id_,string role,string Button)
		{
			ModelState.MarkAllFieldsAsSkipped();
			if (role is null)
			{
				ModelState.AddModelError(string.Empty, "Please provide a correct role.");
				return Page();
			}
			else
			{
				if (Button == "Remove")
				{
					var result = await _userManager.RemoveFromRoleAsync(await _userManager.FindByIdAsync(id_), role);
					if (result.Succeeded)
					{
						return RedirectToPage("/Account/Admin/Index");
					}
					else
					{
						result.Errors.ToList().ForEach(e => ModelState.AddModelError(e.Code, e.Description));
						return Page();
					}
				}
				else if (Button == "Add")
				{
					var result = await _userManager.AddToRoleAsync(await _userManager.FindByIdAsync(id_), role);
					if (result.Succeeded)
					{
						return RedirectToPage("/Account/Admin/Index");
					}
					else
					{
						result.Errors.ToList().ForEach(e => ModelState.AddModelError(e.Code, e.Description));
						return Page();
					}
				}
				ModelState.AddModelError(string.Empty, "invalid Button.");
				return Page();
			}
		}
		public async Task<IActionResult> OnPostValidUsername([FromBody]Jdata data)
		{
			return new JsonResult((await _userManager.FindByNameAsync(data.username)) is null|| (await _userManager.FindByNameAsync(data.username)).Id== data.id);
		}
		public async Task<IActionResult> OnPostValidRole([FromBody] Jdata_ data)
		{
			return new JsonResult((await _userManager.IsInRoleAsync((await _userManager.FindByIdAsync(data.id)),data.role)));
		}
	}
}
