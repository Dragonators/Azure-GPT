using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using OpenAI.Extensions;
using OpenAi_API.Model;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;

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
            builder.Services.AddSwaggerGen();
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
	            //oidc的服务地址(一定要用https!!)
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
	            //添加授权策略，名称为MyApiScope
	            p.AddPolicy("OpenAIApiScope", opt =>
	            {
					//配置鉴定用户的规则，这里表示策略要求用户通过身份认证
					opt.RequireAuthenticatedUser();
		            //鉴定api范围的规则,这里表示策略要求用户具有名为 "scope" 的声明，其值为 "weather_api"
		            opt.RequireClaim("scope", "weather_api");
	            });
            });

			var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.UseCors("AllowAll");

			app.MapControllers().RequireAuthorization("OpenAIApiScope");//为路由系统中的所有 控制器API 端点设置策略;

			app.Run();
        }
    }
}
