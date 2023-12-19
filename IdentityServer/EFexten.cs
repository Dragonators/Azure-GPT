using System.ComponentModel.DataAnnotations;
using System.Reflection;
using IdentityServer.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Serilog;


namespace IdentityServer
{
	public static class EFexten
	{
		static DbContextOptionsBuilder<ApplicationDbContext> optbuilder;
		public static void SetAutoChangeParam<T>(this UserManager<T> userManager, bool param) where T : class
		{
			var Store = typeof(UserManager<T>).GetProperty("Store", BindingFlags.NonPublic|BindingFlags.Instance).GetValue(userManager);
			var auto=Store.GetType().GetProperty("AutoSaveChanges");
			auto.SetValue(Store, param);
		}
		public static void SetStoreContext<T>(this UserManager<T> userManager) where T : class
		{
			var Store = typeof(UserManager<T>).GetProperty("Store", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(userManager);
			var Context = Store.GetType().GetProperty("Context");
			Context.SetValue(Store, Activator.CreateInstance(Context.PropertyType, new object[] { optbuilder.Options }));
		}
		public static void SetStoreContext<T>(this UserManager<T> userManager,ApplicationDbContext db) where T : class
		{
			var Store = typeof(UserManager<T>).GetProperty("Store", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(userManager);
			var Context = Store.GetType().GetProperty("Context");
			Context.SetValue(Store,db);
		}
		static EFexten()
		{
			optbuilder = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseSqlServer(HostingExtensions.Sqlbuilder.ConnectionString);
		}
	}
	public class BirthDayAttribute : ValidationAttribute
	{
		protected override ValidationResult IsValid(object value, ValidationContext validationContext)
		{
			if(value==null)
				return ValidationResult.Success;
			DateTime? date = Convert.ToDateTime(value);
			return (date != null && date < DateTime.Now && date> DateTime.ParseExact("1850-01-01", "yyyy-MM-dd", null)) ? ValidationResult.Success : new ValidationResult("生日必须晚于1850/1/1且不能大于当前时间");
		}
	}

}

