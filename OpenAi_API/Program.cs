using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OpenAI.Extensions;
using OpenAi_API.Filter;
using OpenAi_API.Model;

namespace OpenAi_API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            //配置Swagger认证方式获取Toekn
            builder.Services.AddSwaggerGen(opt =>
            {
				opt.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
					Type=SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows
                    {
                        Implicit = new OpenApiOAuthFlow
                        {
                            AuthorizationUrl = new Uri("https://localhost:5001/connect/authorize"),
                            Scopes = new Dictionary<string, string>
                            {
                                {"chatcompletion_api", "Chatcompletion_API"},
                                {"offline_access", "offline_access"},
                            },
                        },
                    }

                });
                opt.OperationFilter<AuthorizeCheckOperationFilter>();
            });
            builder.Services.AddOpenAIService();
            builder.Services.AddDbContext<ChatDbContext>(options =>
            {
                var Sqlbuilder = new SqlConnectionStringBuilder();
				Sqlbuilder.DataSource = @"DESKTOP-CABO6T6";
				Sqlbuilder.IntegratedSecurity = true;
				Sqlbuilder.InitialCatalog = "ChatServer";
				Sqlbuilder.TrustServerCertificate = true;
				options.UseSqlServer(Sqlbuilder.ConnectionString);
			});
            //CORS
            builder.Services.AddCors(options =>
            {
	            options.AddPolicy("AllowSpecificOrigin",
		            builder =>
		            {
			            
			            builder.WithOrigins("https://localhost:5002")
				            .AllowAnyHeader()
				            .AllowAnyMethod();
		            });
	            options.AddPolicy("AllowAll",
		            builder =>
		            {
			            builder.AllowAnyOrigin()
				            .AllowAnyHeader()
				            .AllowAnyMethod();
		            });
			});
            builder.Services.AddAuthentication("Bearer").AddJwtBearer("Bearer", p =>
            {
	            //oidc的服务地址
	            p.Authority = "https://localhost:5001";//也就是IdentifyServer项目运行地址
	            //设置jwt的验证参数(默认情况下是不需要此验证的)
	            p.TokenValidationParameters = new TokenValidationParameters
	            {
		            ValidateAudience = false
	            };
                p.TokenValidationParameters.ValidTypes = new[] { "at+jwt" };
            });
            builder.Services.AddAuthorization(p =>
            {
                //添加授权策略，名称为ChatApi
                p.AddPolicy("ChatApi", opt =>
	            {
					//配置鉴定用户的规则，这里表示策略要求用户通过身份认证
					opt.RequireAuthenticatedUser();
                    //鉴定api范围的规则,这里表示策略要求用户具有名为 "scope" 的声明，其值为 "chatcompletion_api"
                    opt.RequireClaim("scope", "chatcompletion_api");
	            });
            });
            //使用内存缓存
            builder.Services.AddMemoryCache();

			var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(
                    setup =>
                    {
                        setup.OAuthClientId("swagger_api");
                    });
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.UseCors("AllowAll");

            app.MapControllers();//.RequireAuthorization("OpenAIApiScope");//为路由系统中的所有 控制器API 端点设置策略;

			app.Run();
        }
    }
}
