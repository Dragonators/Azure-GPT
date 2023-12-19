using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace IdentityServer.Model
{
	public class ApplicationUser:IdentityUser
	{
		[Required]
		public string NickName { get; set; }
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
		[RegularExpression(@"^[\u4e00-\u9fa5\w\-\s]+$", ErrorMessage = "街道名只能包含字母,中文,-_符号和空格")]
		public string street_address { get; set; }
		[RegularExpression(@"^[a-zA-Z\u4e00-\u9fa5]+$", ErrorMessage = "地区名只能包含字母和中文")]
		public string locality { get; set; }
		[RegularExpression(@"^\d+(-\d+)?$", ErrorMessage = "邮编只能包含数字和中间的-符号")]
		public string postal_code { get; set; }
		[RegularExpression(@"^[a-zA-Z\u4e00-\u9fa5]+$", ErrorMessage = "国家名只能包含字母和中文")]
		public string country { get; set; }
	}
}
