// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.


using System.ComponentModel.DataAnnotations;

namespace IdentityServerHost.Pages.Register;
public class RegisterInputModel
{
    [Required]
    public string Username { get; set; }
        
    [Required]
    public string Password { get; set; }
	[Required]
	public string Name { get; set; }

	public string ReturnUrl { get; set; }

    public string Button { get; set; }
}