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
            //����Swagger��֤��ʽ��ȡToekn
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
	            //oidc�ķ����ַ
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
                //�����Ȩ���ԣ�����ΪChatApi
                p.AddPolicy("ChatApi", opt =>
	            {
					//���ü����û��Ĺ��������ʾ����Ҫ���û�ͨ�������֤
					opt.RequireAuthenticatedUser();
                    //����api��Χ�Ĺ���,�����ʾ����Ҫ���û�������Ϊ "scope" ����������ֵΪ "chatcompletion_api"
                    opt.RequireClaim("scope", "chatcompletion_api");
	            });
            });
            //ʹ���ڴ滺��
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

            app.MapControllers();//.RequireAuthorization("OpenAIApiScope");//Ϊ·��ϵͳ�е����� ������API �˵����ò���;

			app.Run();
        }
    }
}
