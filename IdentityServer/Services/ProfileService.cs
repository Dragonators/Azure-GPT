using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using IdentityModel;
using IdentityServer.Model;
using Microsoft.AspNetCore.Identity;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text.Json;

namespace IdentityServer.Services
{
    public class ProfileService : IProfileService
    {
        protected UserManager<ApplicationUser> _userManager;
        protected RoleManager<IdentityRole> _roleManager;

        public ProfileService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            //>Processing
            var user = await _userManager.GetUserAsync(context.Subject);

            var claims = new List<Claim>
            {   
                new Claim(JwtClaimTypes.WebSite,user.Website ?? string.Empty),
                new Claim(JwtClaimTypes.PreferredUserName,user.UserName),
                new Claim(JwtClaimTypes.Email,user.Email ?? string.Empty),
                new Claim(JwtClaimTypes.Address,user.Address ?? string.Empty),
                new Claim(JwtClaimTypes.NickName,user.NickName),
                new Claim(JwtClaimTypes.GivenName,user.GivenName ?? string.Empty),
                new Claim(JwtClaimTypes.FamilyName,user.FamilyName ?? string.Empty),
                new Claim(JwtClaimTypes.Name,$"{user.GivenName} {user.FamilyName}"),

            };
            foreach(var role in await _userManager.GetRolesAsync(user))
            {
                claims.Add(new Claim(JwtClaimTypes.Role, role));
            }
            context.IssuedClaims.AddRange(claims);
        }

        public async Task IsActiveAsync(IsActiveContext context)
        {
            //>Processing
            var user = await _userManager.GetUserAsync(context.Subject);

            context.IsActive = (user != null) && await Task.FromResult(true); ;
        }
    }
}
