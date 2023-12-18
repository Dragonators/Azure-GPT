// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.


using IdentityServer.Model;
using System.ComponentModel.DataAnnotations;

namespace IdentityServerHost.Pages.Admin;
public class CreateInputModel
{
    [Required(ErrorMessage = "用户名是必填的")]
    public string Username { get; set; }
	[Required]    
    public string Password { get; set; }

	public string GivenName { get; set; }

	public string FamilyName { get; set; }
	public int? Sex { get; set; }
	[Url]
	public string Website { get; set; }
	public Address_ Address { get; set; }
	public DateTime? Birth { get; set; }
	[EmailAddress]
	public string Email { get; set; }
}