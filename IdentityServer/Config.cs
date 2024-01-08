using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using IdentityServer.Model;
using IdentityServerHost.Pages.Admin;
using Mapster;
using MapsterMapper;
using System.Text.Json;

namespace IdentityServer
{
	public static class Config
	{
		public static IEnumerable<IdentityResource> IdentityResources =>
			new IdentityResource[]
			{
				new IdentityResources.OpenId(),
				new IdentityResources.Profile()
            };

		public static IEnumerable<ApiScope> ApiScopes =>
			new ApiScope[]
			{
				new ApiScope()
				{
				Name = "chatcompletion_api",
				DisplayName = "ChatCompletion_API"
                },
                new ApiScope()
                {
                Name = "profile_api",
                DisplayName = "Profile_API"
                }
            };
		public static IEnumerable<ApiResource> ApiResources =>
			new ApiResource[]
			{
				new ApiResource("openai_resource", "OpenAi_Resource")
				{
					Scopes={ "chatcompletion_api" }
				}
			};

		public static IEnumerable<Client> Clients =>
			new Client[]
			{
				new Client()
				{
                    ClientId = "Profile_Client",
                    ClientSecrets = new List<Secret>()
                    {
                        new Secret("secret".Sha256())
                    },
                    AllowedGrantTypes = new List<string>(){GrantType.ClientCredentials},// 没有交互式用户，使用 clientid/secret 进行身份验证
					AllowedScopes = { "profile_api" }//客户端允许访问的api,可以有多个
				},
				//配置OICD Client
				new Client()
				{
					ClientId = "web",
					ClientName = "Web Client",
					ClientSecrets = new List<Secret>()
					{
						new Secret("secret".Sha256())
					},
					AllowedGrantTypes = new List<string>(){GrantType.AuthorizationCode}, //授权码许可协议
					AllowOfflineAccess = true,//启用对刷新令牌 refreshToken的支持
					//登录成功的跳转地址
					RedirectUris = {"https://localhost:5002/signin-oidc","https://oauth.pstmn.io/v1/callback"}, //客户端的地址，signin-oidc:标准协议里的端点名称
					//登出后的跳转地址
					PostLogoutRedirectUris = {"https://localhost:5002/signout-callback-oidc"},
					AllowedScopes =
					{
						IdentityServerConstants.StandardScopes.OpenId,
						IdentityServerConstants.StandardScopes.Profile, //客户端后端的Openid Connect默认添加这两项，也需要在AllowedScopes中添加
                        "chatcompletion_api"
                    },//允许访问的api,可以有多个
					RequireConsent = true //是否需要用户点同意
				},
                //配置Swagger_Postman Client
				new Client()
				{
					ClientId = "swagger_api",
					ClientName = "Swagger_Api",
					AllowedGrantTypes = GrantTypes.Implicit,
                    AllowOfflineAccess = true,
					AllowAccessTokensViaBrowser=true,
					RedirectUris = { "https://oauth.pstmn.io/v1/callback","https://localhost:7001/swagger/oauth2-redirect.html"},
                    AllowedScopes =
					{
                        "chatcompletion_api"
                    }
				}
			};
		//将上传的CreateInputModel映射到ApplicationUser
		public static void MapUser(CreateInputModel input, ref ApplicationUser user)
		{
			var config = new TypeAdapterConfig();
			config.ForType<CreateInputModel, ApplicationUser>()
				.Map(dest=>dest.UserName,src=>src.Username)
				.Map(dest=>dest.NickName,src=>src.NickName)
				.Map(dest=>dest.GivenName,src=>src.GivenName)
				.Map(dest=>dest.FamilyName,src=>src.FamilyName)
				.Map(dest=>dest.Sex,src=>src.Sex)
				.Map(dest=>dest.Website,src=>src.Website)
				.Map(dest=>dest.Birth,src=>src.Birth)
				.Map(dest=>dest.Email,src=>src.Email)
				.Map(dest => dest.Address, src => JsonSerializer.Serialize(src.Address,new JsonSerializerOptions()))
				.IgnoreNonMapped(true)
				.IgnoreNullValues(true);
			var mapper = new Mapper(config);
			mapper.Map(input, user);
		}
	}
}