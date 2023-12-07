using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Mappers;
using IdentityServer.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace IdentityServer
{
	internal static class HostingExtensions
	{
		public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
		{
			var migrationsAssembly = typeof(Program).Assembly.GetName().Name;

			var Sqlbuilder=new SqlConnectionStringBuilder();
			Sqlbuilder.DataSource = @"sha-xhji-d1\SQLEXPRESS";
			Sqlbuilder.IntegratedSecurity = true;
			Sqlbuilder.InitialCatalog = "IdentityServer";
			Sqlbuilder.TrustServerCertificate = true;//?

			builder.Services.AddRazorPages();//VIEW

			builder.Services.AddDbContext<ApplicationDbContext>(options =>
				options.UseSqlServer(Sqlbuilder.ConnectionString));
			builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
			.AddEntityFrameworkStores<ApplicationDbContext>()
			.AddDefaultTokenProviders();

			builder.Services.AddIdentityServer(options =>
				{
					// https://docs.duendesoftware.com/identityserver/v6/fundamentals/resources/api_scopes#authorization-based-on-scopes
					options.EmitStaticAudienceClaim = true;
				})
				/*
				.AddInMemoryApiScopes(Config.ApiScopes)
				.AddInMemoryClients(Config.Clients)
				.AddTestUsers(TestUsers.Users)
				.AddInMemoryApiResources(Config.ApiResources)
				.AddInMemoryIdentityResources(Config.IdentityResources);
				*/
				.AddConfigurationStore(opt =>
				{
					opt.ConfigureDbContext = d => d.UseSqlServer(Sqlbuilder.ConnectionString,
						sql => sql.MigrationsAssembly(migrationsAssembly));//search
				})
				.AddOperationalStore(opt =>
				{
					opt.ConfigureDbContext = d => d.UseSqlServer(Sqlbuilder.ConnectionString,
						sql => sql.MigrationsAssembly(migrationsAssembly));
					opt.EnableTokenCleanup = true;
				})
				.AddAspNetIdentity<ApplicationUser>();

			return builder.Build();
		}

		public static WebApplication ConfigurePipeline(this WebApplication app)
		{
			app.UseSerilogRequestLogging();

			if (app.Environment.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			SeedData.InitializeDatabase(app);
			app.UseStaticFiles();//VIEW
			app.UseRouting();//VIEW

			app.UseIdentityServer();


			app.UseAuthorization();
			app.MapRazorPages().RequireAuthorization();//VIEW

			return app;
		}
	}
}
