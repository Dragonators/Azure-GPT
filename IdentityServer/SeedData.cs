using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Mappers;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services.KeyManagement;
using IdentityServer.Model;
using Microsoft.AspNetCore.Identity;
using Serilog;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.Json;

namespace IdentityServer
{
	public class SeedData
	{
		public static async Task InitializeDatabase(IApplicationBuilder app)
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
			await InitializeUserAsync(app.ApplicationServices.CreateScope().ServiceProvider);
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
			await EnsureAdminAsync(userManager,services);
		}
		private static async Task EnsureRolesAsync(RoleManager<IdentityRole> roleManager)
		{
			var alreadyExists = await roleManager
				.RoleExistsAsync(Constants.Admin);
			if (alreadyExists) return;

			await roleManager.CreateAsync(new IdentityRole(Constants.Admin));
		}
		private static async Task EnsureAdminAsync(UserManager<ApplicationUser> userManager, IServiceProvider services)
		{
			//if ((await userManager.Users
			//	.Where(x => x.UserName == "bob")
			//	.SingleOrDefaultAsync()) 
			//	!= null) 
			//return;
			//Stopwatch mywatch = Stopwatch.StartNew();
			try
			{
				var baseUser = AdminUsers.Users.First();

				for (int i = 0; i < 200; i++)
				{
					string s = JsonSerializer.Serialize(baseUser);
					var iu = JsonSerializer.Deserialize<ApplicationUser>(s);
					iu.UserName += i.ToString();
					iu.Id = Guid.NewGuid().ToString();
					AdminUsers.Users.Add(iu);
				}

				Stopwatch mywatch = Stopwatch.StartNew();
				foreach (var user in AdminUsers.Users)
				{
					await userManager.CreateAsync(user, Constants.pwd);
					await userManager.AddToRoleAsync(user, Constants.Admin);
				}
				mywatch.Stop();
				Log.Information($"First cost {mywatch}");
				//await DelAdmin(userManager);
			}
			catch (Exception ex)
			{
				Log.Information(ex.Message);
			}
			finally
			{
				//var listOfTasks = new List<Task>();
				//foreach (var user in userManager.Users.ToList())
				//{					
				//	listOfTasks.Add(userManager.DeleteAsync(user));
				//}
				//await Task.WhenAll(listOfTasks);
				foreach (var user in userManager.Users.ToList())
				{
					await userManager.DeleteAsync(user);
				}
			}
		}
		private static async Task DelAdmin(UserManager<ApplicationUser> userManager)
		{
			var Admins = await userManager.GetUsersInRoleAsync(Constants.Admin);
			var listOfTasks = new List<Task>();
			foreach (var user in Admins)
			{
				listOfTasks.Add(userManager.DeleteAsync(user));
			}
			await Task.WhenAll(listOfTasks);
		}
		private static class Constants
		{
			public const string Admin = "Administrator";
			public const string pwd = "123";
		}

	}
}
