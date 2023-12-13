using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace IdentityServer.Model
{
	public class ApplicationUser:IdentityUser
	{
		[Required]
		public string Name { get; set; }
		public string GivenName { get; set; }
		public string FamilyName { get; set; }
		public int? Sex { get; set; }
		public int? Age { get; set; }
		public string Website { get; set; }
		public string Address { get; set; }
		public DateTime? Birth { get; set; }
		public bool tdIsDelete { get; set; }
	}
	public class Address_
	{
		public string street_address { get; set; }
        public string locality { get; set; }
		public int postal_code { get; set; }
		public string country { get; set; }

    }
}
