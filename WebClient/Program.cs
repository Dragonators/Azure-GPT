using IdentityModel;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
JwtSecurityTokenHandler.DefaultMapInboundClaims = false;
builder.Services.AddRazorPages();
builder.Services.AddAuthentication(opt =>
{
	opt.DefaultScheme = "Cookies";
	opt.DefaultChallengeScheme = "oidc";
})
	.AddCookie("Cookies", opt =>
	{
		opt.Events.OnSigningOut = async e =>
		{
			// revoke refresh token on sign-out
			await e.HttpContext.RevokeUserRefreshTokenAsync();
		};
	})
	.AddOpenIdConnect("oidc", opt =>
	{
		opt.Authority = "https://localhost:5001";
		opt.ClientId = "web";
		opt.ClientSecret = "secret";
		opt.ResponseType = "code";
		opt.SaveTokens = true;

		//Api Scopes
        opt.Scope.Add("chatcompletion_api");

        opt.Scope.Add("offline_access");

        /*
         * Default Actioins of OpenIdConnectOptions
         * this.ClaimActions.MapUniqueJsonKey("sub", "sub");
         * this.ClaimActions.MapUniqueJsonKey("name", "name");
         * this.ClaimActions.MapUniqueJsonKey("given_name", "given_name");
         * this.ClaimActions.MapUniqueJsonKey("family_name", "family_name");
         * this.ClaimActions.MapUniqueJsonKey("profile", "profile");
         * this.ClaimActions.MapUniqueJsonKey("email", "email");
         */

        //userinfo to local Userclaims
        opt.ClaimActions.MapUniqueJsonKey("website", JwtClaimTypes.WebSite);
        opt.ClaimActions.MapUniqueJsonKey("Nick Name", JwtClaimTypes.NickName);
        opt.ClaimActions.MapUniqueJsonKey("UserName", JwtClaimTypes.PreferredUserName);
        opt.ClaimActions.MapUniqueJsonKey("Address", JwtClaimTypes.Address);
        opt.ClaimActions.MapJsonKey("role", JwtClaimTypes.Role);
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            NameClaimType = "name",
            RoleClaimType = "role"
        };

        opt.GetClaimsFromUserInfoEndpoint = true;
	});
builder.Services.AddAccessTokenManagement(options =>
{
	options.Client.Clients.Add("client1", new ClientCredentialsTokenRequest
	{
		Address = "https://localhost:5001/connect/token",
		ClientId = "client",
		ClientSecret = "secret",
		Scope = "chatcompletion_api"
	});
});
builder.Services.AddUserAccessTokenHttpClient("Chat_client", configureClient: client =>
{
	client.BaseAddress = new Uri("https://localhost:7001/");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages().RequireAuthorization();
app.MapGet("/UserToken", async (HttpContext httpcontext) =>
{
    var token = await httpcontext.GetUserAccessTokenAsync();
    if (token != null)
    {
        return token;
    }
    return "";
}).RequireAuthorization();;

app.Run();
