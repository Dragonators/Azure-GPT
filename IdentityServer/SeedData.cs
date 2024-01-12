using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Mappers;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services.KeyManagement;
using IdentityServer.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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
		private static void ChangeData_(IServiceScopeFactory serviceScopeFactory)//just single context,sync foreach,fast
		{
			using (var context = serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<ConfigurationDbContext>())
			{
                context.RemoveRange(context.IdentityResources);
                context.RemoveRange(context.Clients);
                context.RemoveRange(context.ApiScopes);
				context.RemoveRange(context.ApiResources);
                context.SaveChanges();

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
            if (!alreadyExists) await roleManager.CreateAsync(new IdentityRole(Constants.Admin));
            var alreadyExists_ = await roleManager
                .RoleExistsAsync(Constants.User);
            if (!alreadyExists_) await roleManager.CreateAsync(new IdentityRole(Constants.User));
		}
		private static async Task EnsureAdminAsync(UserManager<ApplicationUser> userManager, IServiceProvider services)
		{
            if ((await userManager.GetUsersInRoleAsync(Constants.Admin)).Any())
				return;
			
			try//add admins
			{
				userManager.SetAutoChangeParam<ApplicationUser>(false);
				var baseUser = AdminUsers.Users.First();
				//only need one,followed code Just for testing performance of large amount of data initialization
				for (int i = 1; i < 2; i++)
				{
					string s = JsonSerializer.Serialize(baseUser);
					var iu = JsonSerializer.Deserialize<ApplicationUser>(s);
					iu.UserName += i.ToString();
					iu.Id = Guid.NewGuid().ToString();
					AdminUsers.Users.Add(iu);
				}
				foreach (var user in AdminUsers.Users)
				{
					await userManager.CreateAsync(user, Constants.pwd);
				}
				await services.GetRequiredService<ApplicationDbContext>().SaveChangesAsync();
				foreach (var user in AdminUsers.Users)
				{
					await userManager.AddToRoleAsync(user, Constants.Admin);
				}
				await services.GetRequiredService<ApplicationDbContext>().SaveChangesAsync();
			}
			catch (Exception ex)
			{
				Log.Information(ex.Message);
			}
			finally
			{
                //await DelAll(userManager, services);
                userManager.SetAutoChangeParam<ApplicationUser>(true);
            }
		}
		private static async Task DelAll(UserManager<ApplicationUser> userManager, IServiceProvider services)
		{
			foreach (var user in userManager.Users.ToList())
			{
				await userManager.DeleteAsync(user);
			}
			await services.GetRequiredService<ApplicationDbContext>().SaveChangesAsync();
			userManager.SetAutoChangeParam<ApplicationUser>(true);
		}
		private static class Constants
		{
			public const string Admin = "Administrator";
            public const string User = "BasicUser";
            public const string pwd = "123";
		}
	}
}
