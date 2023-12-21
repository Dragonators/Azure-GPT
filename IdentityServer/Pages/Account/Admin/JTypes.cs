namespace IdentityServerHost.Pages.Admin
{
	/// <summary>
	/// 获取js fetch发送的数据，用于验证用户名重复
	/// </summary>
	public class Jdata
	{
		public string id { get; set; }
		public string username { get; set; }
	}
	/// <summary>
	/// 获取js fetch发送的数据，用于验证权限重复
	/// </summary>
	public class Jdata_
	{
		public string id { get; set; }
		public string role { get; set; }
	}
}
