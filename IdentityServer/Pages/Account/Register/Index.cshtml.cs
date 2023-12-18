using Duende.IdentityServer.Events;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Stores;
using IdentityServer.Model;
using IdentityServerHost.Pages.Login;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.VisualBasic;
using Serilog;

namespace IdentityServerHost.Pages.Register
{
	[SecurityHeaders]
	[AllowAnonymous]
    public class IndexModel : PageModel
    {
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly IIdentityServerInteractionService _interaction;
		[BindProperty]
		public RegisterInputModel Input { get; set; }
		public IndexModel(
		IIdentityServerInteractionService interaction,
		UserManager<ApplicationUser> userManager)
		{
			_userManager = userManager;
			_interaction = interaction;
		}


		public async Task OnGet(string returnUrl)
        {
			await BuildModelAsync(returnUrl);
		}
        public async Task<IActionResult> OnPostCreate()
        {
            //Log.Information("1");
			var context = await _interaction.GetAuthorizationContextAsync(Input.ReturnUrl);
			if (Input.Button != "create")
			{
				if (context != null)
				{
					await _interaction.DenyAuthorizationAsync(context, AuthorizationError.AccessDenied);
					if (context.IsNativeClient())
					{
						return this.LoadingPage(Input.ReturnUrl);
					}

					return Redirect(Input.ReturnUrl);
				}
				else
				{
					return Redirect("~/");
				}
			}
			if (ModelState.IsValid)
			{
				var result = await _userManager.CreateAsync(new ApplicationUser
				{
					UserName = Input.Username,
					Name=Input.Name
				},Input.Password);
				if (result.Succeeded)
				{
					result = await _userManager.AddToRoleAsync(await _userManager.FindByNameAsync(Input.Username), "BasicUser");
					if (result.Succeeded)
					{
						if (context != null)
						{
							if (context.IsNativeClient())
							{
								return this.LoadingPage(Input.ReturnUrl);
							}

							return Redirect(Input.ReturnUrl);
						}

						if (Url.IsLocalUrl(Input.ReturnUrl))
						{
							return Redirect(Input.ReturnUrl);
						}
						else if (string.IsNullOrEmpty(Input.ReturnUrl))
						{
							return Redirect("/Account/Login");
						}
						else
						{
							throw new Exception("invalid return URL");
						}
					}
					//else?
					//
					//
				}
				ModelState.AddModelError(string.Empty, "Invalid username or password or Name");
			}
			await BuildModelAsync(Input.ReturnUrl);
			return Page();
		}
		private async Task BuildModelAsync(string returnUrl)
		{
			Input = new RegisterInputModel
			{
				ReturnUrl = returnUrl
			};
		}
	}
}