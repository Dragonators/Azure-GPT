using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthentication("Bearer").AddJwtBearer("Bearer", p =>
{
    //oidc的服务地址(一定要用https!!)
    p.Authority = "https://localhost:5001";//也就是IdentifyServer项目运行地址
                                           //设置jwt的验证参数(默认情况下是不需要此验证的)
    p.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateAudience = false
    };
});
builder.Services.AddAuthorization(p =>
{
    //添加授权策略，名称为MyApiScope
    p.AddPolicy("MyApiScope", opt =>
    {
        //配置鉴定用户的规则，这里表示策略要求用户通过身份认证
        opt.RequireAuthenticatedUser();
        //鉴定api范围的规则,这里表示策略要求用户具有名为 "scope" 的声明，其值为 "simple_api"
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

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers().RequireAuthorization("MyApiScope");//为路由系统中的所有 控制器API 端点设置策略

app.Run();
