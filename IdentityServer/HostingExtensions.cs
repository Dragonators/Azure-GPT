using IdentityServer.Model;
using IdentityServer.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace IdentityServer
{
	internal static class HostingExtensions
	{
		public static SqlConnectionStringBuilder Sqlbuilder { get; set; }
		private static string strc;
		public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
		{
			var migrationsAssembly = typeof(Program).Assembly.GetName().Name;



			builder.Services.AddRazorPages();//VIEW

			builder.Services.AddDbContext<ApplicationDbContext>(options =>
				options.UseSqlServer(builder.Configuration.GetConnectionString("Home_IDP")));
			builder.Services.AddIdentity<ApplicationUser, IdentityRole>(opt =>
			{
				opt.Password.RequireNonAlphanumeric = false;
				opt.Password.RequiredLength = 1;
				opt.Password.RequireUppercase = false;
				opt.Password.RequireLowercase = false;
				opt.Password.RequireDigit = false;
			})
			.AddEntityFrameworkStores<ApplicationDbContext>()
			.AddDefaultTokenProviders();

			builder.Services.AddIdentityServer(options =>
				{
					// https://docs.duendesoftware.com/identityserver/v6/fundamentals/resources/api_scopes#authorization-based-on-scopes
					options.EmitStaticAudienceClaim = true;
                })
				.AddConfigurationStore(opt =>
				{
					opt.ConfigureDbContext = d => d.UseSqlServer(builder.Configuration.GetConnectionString("Home_IDP"),
						sql => sql.MigrationsAssembly(migrationsAssembly));//search
				})
				.AddOperationalStore(opt =>
				{
					opt.ConfigureDbContext = d => d.UseSqlServer(builder.Configuration.GetConnectionString("Home_IDP"),
						sql => sql.MigrationsAssembly(migrationsAssembly));
					opt.EnableTokenCleanup = true;
				})
				.AddAspNetIdentity<ApplicationUser>()
                .AddProfileService<ProfileService>();
			return builder.Build();
		}

		public static WebApplication ConfigurePipeline(this WebApplication app)
		{
			app.UseSerilogRequestLogging();

			if (app.Environment.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			app.UseStaticFiles();//VIEW
			app.UseRouting();//VIEW

            app.UseIdentityServer();

            app.UseHttpsRedirection();
			app.UseAuthorization();
			app.MapRazorPages().RequireAuthorization();//VIEW

            return app;
		}
	}
}
