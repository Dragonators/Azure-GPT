using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebClient.Model;

namespace WebClient.Pages
{
	public class UsersModel : PageModel
    {
		private readonly UserManager<ApplicationUser> _userManager;
		public async Task OnGet()
		{
			
		}
    }
}
