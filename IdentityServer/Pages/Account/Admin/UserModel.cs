using IdentityServer.Model;
using IdentityServerHost.Pages.Grants;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace IdentityServerHost.Pages.Admin
{
    public class UserModel
    {
        public IEnumerable<ApplicationUser> Users { get; set; }
        public UserManager<ApplicationUser> userManager { get; set; }
    }
}
