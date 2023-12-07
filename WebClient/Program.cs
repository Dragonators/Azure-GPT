using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using System.IdentityModel.Tokens.Jwt;

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

		opt.Scope.Add("profile");
		opt.Scope.Add("weather_api");
		opt.Scope.Add("email");
		opt.Scope.Add("custom_resource");
		opt.Scope.Add("offline_access");
        opt.ClaimActions.MapUniqueJsonKey("website", "website");
        opt.ClaimActions.MapUniqueJsonKey("address", "address");
        opt.ClaimActions.MapUniqueJsonKey("phone_number", "phone_number");

        opt.GetClaimsFromUserInfoEndpoint = true;
	});
builder.Services.AddAccessTokenManagement(options =>
{
	options.Client.Clients.Add("client1", new ClientCredentialsTokenRequest
	{
		Address = "https://localhost:5001/connect/token",
		ClientId = "client",
		ClientSecret = "secret",
		Scope = "weather_api"
	});
});
builder.Services.AddClientAccessTokenHttpClient("weather_client", "client1", configureClient: client =>
{
    client.BaseAddress = new Uri("https://localhost:6001/");
});
builder.Services.AddUserAccessTokenHttpClient("user_client",configureClient: client =>
{
	client.BaseAddress = new Uri("https://localhost:6001/");
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

app.Run();
