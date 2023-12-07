using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Mappers;
using Duende.IdentityServer.Models;
using IdentityServer.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IdentityServer
{
	public class SeedData
	{
		public static void InitializeDatabase(IApplicationBuilder app)
		{
			/*using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
			{
				//IdentityServer's default DbContext Migration	
				//Just need once
				serviceScope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();

				var context = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
				context.Database.Migrate();//=Update database
			    //
				var Usercontext = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
				var Configcontext = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
				var pendingMigrations = Usercontext.Database.GetPendingMigrations();
			    if (pendingMigrations.Any())
				{
					Usercontext.Database.Migrate();
				}
		    }*/
			//ChangeData(app.ApplicationServices.GetService<IServiceScopeFactory>());
			ChangeData_(app.ApplicationServices.GetService<IServiceScopeFactory>());
		}
		//ConfigurationDbContext Data Change
		private static void ChangeData(IServiceScopeFactory serviceScopeFactory)
		{
			Parallel.Invoke(() =>
			{
				using (var context = serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<ConfigurationDbContext>())
				{
					foreach (var identityResource in Config.IdentityResources)
					{
						if (!context.IdentityResources.Any(i => i.Name == identityResource.Name))
						{
							context.IdentityResources.Add(identityResource.ToEntity());
						}
					}
					context.SaveChanges();
				}
			}, () =>
			{
				using (var context = serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<ConfigurationDbContext>())
				{
					foreach (var Client in Config.Clients)
					{
						if (!context.Clients.Any(i => i.ClientId == Client.ClientId))
						{
							context.Clients.Add(Client.ToEntity());
						}
					}
					context.SaveChanges();
				}
			}, () =>
			{
				using (var context = serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<ConfigurationDbContext>())
				{
					foreach (var ApiScope in Config.ApiScopes)
					{
						if (!context.ApiScopes.Any(i => i.Name == ApiScope.Name))
						{
							context.ApiScopes.Add(ApiScope.ToEntity());
						}
					}
					context.SaveChanges();
				}
			}, () =>
			{
				using (var context = serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<ConfigurationDbContext>())
				{
					foreach (var resource in Config.ApiResources)
					{
						if (!context.ApiResources.Any(i => i.Name == resource.Name))
						{
							context.ApiResources.Add(resource.ToEntity());
						}
					}
					context.SaveChanges();
				}
			});
		}//create several scopes,Parrllel foreach,slow
		private static void ChangeData_(IServiceScopeFactory serviceScopeFactory)//just single context,sync foreach,fast
		{
			using (var context = serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<ConfigurationDbContext>())
			{
				foreach (var identityResource in Config.IdentityResources)
				{
					if (!context.IdentityResources.Any(i => i.Name == identityResource.Name))
					{
						context.IdentityResources.Add(identityResource.ToEntity());
					}
				}
				foreach (var Client in Config.Clients)
				{
					if (!context.Clients.Any(i => i.ClientId == Client.ClientId))
					{
						context.Clients.Add(Client.ToEntity());
					}
				}
				foreach (var ApiScope in Config.ApiScopes)
				{
					if (!context.ApiScopes.Any(i => i.Name == ApiScope.Name))
					{
						context.ApiScopes.Add(ApiScope.ToEntity());
					}
				}
				foreach (var resource in Config.ApiResources)
				{
					if (!context.ApiResources.Any(i => i.Name == resource.Name))
					{
						context.ApiResources.Add(resource.ToEntity());
					}
				}
				context.SaveChanges();
			}
		}
		public static async Task InitializeUserAsync(IServiceProvider services)
		{
			var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
			await EnsureRolesAsync(roleManager);
			var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
			await EnsureAdminAsync(userManager);
		}
		private static async Task EnsureRolesAsync(RoleManager<IdentityRole> roleManager)
		{
			var alreadyExists = await roleManager
				.RoleExistsAsync(Constants.Admin);
			if (alreadyExists) return;

			await roleManager.CreateAsync(new IdentityRole(Constants.Admin));
		}
		private static async Task EnsureAdminAsync(UserManager<ApplicationUser> userManager)
		{
			var Admin = await userManager.Users
				.Where(x => x. == "admin@todo.local")
				.SingleOrDefaultAsync();
			if (Admin != null) return;
		}
		private static class Constants
		{
			public const string Admin = "Administrator";
		}
	}
}
