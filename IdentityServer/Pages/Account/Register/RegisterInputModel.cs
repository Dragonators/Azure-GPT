// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.


using System.ComponentModel.DataAnnotations;

namespace IdentityServerHost.Pages.Register;
public class RegisterInputModel
{
	[Required(ErrorMessage = "用户名是必填的")]
	[RegularExpression(@"^[\w\-]{2,16}$", ErrorMessage = "用户名只能包含字母、数字和-_，长度不短于2不超过16")]
	public string Username { get; set; }
        
    [Required(ErrorMessage = "密码是必填的")]
    public string Password { get; set; }
	[Required(ErrorMessage = "昵称是必填的")]
	[RegularExpression(@"^[\w\-]{2,16}$", ErrorMessage = "昵称只能包含字母、数字和-_，长度不短于2不超过16")]
	public string Name { get; set; }

	public string ReturnUrl { get; set; }

    public string Button { get; set; }
}