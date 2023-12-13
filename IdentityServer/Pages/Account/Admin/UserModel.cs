using IdentityServer.Model;
using IdentityServerHost.Pages.Grants;

namespace IdentityServer.Pages.Account.Admin
{
    public class UserModel
    {
        public IEnumerable<ApplicationUser> Users { get; set; }
    }
}
