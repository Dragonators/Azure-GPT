using Duende.Bff.Yarp;
using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

builder.Services
    .AddBff()
    .AddRemoteApis();
builder.Services.AddAuthorization();
JwtSecurityTokenHandler.DefaultMapInboundClaims = false;
builder.Services.AddAuthentication(opt =>
{
    opt.DefaultScheme = "Cookies";
    opt.DefaultChallengeScheme = "oidc";
    opt.DefaultSignOutScheme = "oidc";
})
	.AddCookie("Cookies")
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
app.UseBff();
app.UseAuthorization();

app.MapBffManagementEndpoints();

app.MapRazorPages().RequireAuthorization();

app.MapRemoteBffApiEndpoint("/remote", "https://localhost:7001")
    .RequireAccessToken(Duende.Bff.TokenType.User);

app.Run();
