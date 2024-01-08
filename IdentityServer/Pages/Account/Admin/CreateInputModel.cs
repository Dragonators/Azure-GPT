// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.


using IdentityServer;
using IdentityServer.Model;
using System.ComponentModel.DataAnnotations;

namespace IdentityServerHost.Pages.Admin;
public class CreateInputModel
{
    [Required(ErrorMessage = "用户名是必填的")]
	[RegularExpression(@"^[\w\-]{2,16}$", ErrorMessage = "用户名只能包含字母、数字和-_，长度不短于2不超过16")]
    public string Username { get; set; }
	[RegularExpression(@"^[a-zA-Z\u4e00-\u9fa5]+$", ErrorMessage = "名字只能包含汉字或字母")]
	public string GivenName { get; set; }
	[RegularExpression(@"^[a-zA-Z\u4e00-\u9fa5]+$", ErrorMessage = "名字只能包含汉字或字母")]
	public string FamilyName { get; set; }
	[Required(ErrorMessage = "昵称是必填的")]
	[RegularExpression(@"^[\w\-]{2,16}$", ErrorMessage = "昵称只能包含字母、数字和-_，长度不短于2不超过16")]
	public string NickName { get; set; }
	[Range(0,2, ErrorMessage = "姓别只能为男或者女")]
	public int? Sex { get; set; }
	[RegularExpression(@"^\w+[^\s]+(\.[^\s]+){1,}$", ErrorMessage = "请填写正确的Url")]
	public string Website { get; set; }
	public Address_ Address { get; set; }
	[BirthDay]
	public DateTime? Birth { get; set; }
	[EmailAddress(ErrorMessage = "请填写正确的邮箱")]
	public string Email { get; set; }
	public string Id { get; set; }
	
}