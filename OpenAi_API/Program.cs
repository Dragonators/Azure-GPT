using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using OpenAI.Extensions;
using OpenAi_API.Model;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using static System.Net.WebRequestMethods;

namespace OpenAi_API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(opt =>
            {
				opt.AddSecurityDefinition("oidc", new OpenApiSecurityScheme
                {
					Type=SecuritySchemeType.OAuth2,
                    OpenIdConnectUrl = new Uri("https://localhost:5001/.well-known/openid-configuration"),
                    Flows = new OpenApiOAuthFlows
                    {
                        AuthorizationCode = new OpenApiOAuthFlow
                        {
                            AuthorizationUrl = new Uri("https://localhost:5001/connect/authorize"),
                            TokenUrl = new Uri("https://localhost:5001/connect/token"),
                            Scopes = new Dictionary<string, string>
                            {
                                {"weather_api", "weather_api"},
                                {"email", "email"},
                                {"custom_resource", "custom_resource"},
                                {"roles", "roles"},
                                {"offline_access", "offline_access"},
                                {"openid","openid"},
                                {"profile","profile"}
                            },
                            RefreshUrl = new Uri("https://localhost:5001/connect/revocation"),
                        },
                    }

                });
                opt.AddSecurityDefinition("apikey",new OpenApiSecurityScheme
                {
                    Type=SecuritySchemeType.ApiKey,
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Description = "Apikey"
                });
            });
            builder.Services.AddOpenAIService();
            builder.Services.AddDbContext<ChatDbContext>(options =>
            {
                var Sqlbuilder = new SqlConnectionStringBuilder();
				Sqlbuilder.DataSource = @"SHA-XHJI-D1\SQLEXPRESS";
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
	            //oidc�ķ����ַ(һ��Ҫ��https!!)
	            p.Authority = "https://localhost:5001";//Ҳ����IdentifyServer��Ŀ���е�ַ
	            //����jwt����֤����(Ĭ��������ǲ���Ҫ����֤��)
	            p.TokenValidationParameters = new TokenValidationParameters
	            {
		            ValidateAudience = false
	            };
                p.TokenValidationParameters.ValidTypes = new[] { "at+jwt" };
            });
            builder.Services.AddAuthorization(p =>
            {
	            //�����Ȩ���ԣ�����ΪMyApiScope
	            p.AddPolicy("OpenAIApiScope", opt =>
	            {
					//���ü����û��Ĺ��������ʾ����Ҫ���û�ͨ�������֤
					opt.RequireAuthenticatedUser();
		            //����api��Χ�Ĺ���,�����ʾ����Ҫ���û�������Ϊ "scope" ����������ֵΪ "weather_api"
		            opt.RequireClaim("scope", "weather_api");
	            });
            });


			var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(
                    setup =>
                    {
                        setup.OAuthClientId("web");
                        setup.OAuthClientSecret("secret");
                        setup.OAuthUsePkce();
                    }
                    );
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.UseCors("AllowAll");

			app.MapControllers().RequireAuthorization("OpenAIApiScope");//Ϊ·��ϵͳ�е����� ������API �˵����ò���;

			app.Run();
        }
    }
}
