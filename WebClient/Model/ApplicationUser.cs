﻿using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace WebClient.Model
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
}