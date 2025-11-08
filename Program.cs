using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OHCP_BK.Data;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(7267, listenOptions =>
    {
        listenOptions.UseHttps();
    });
});

builder.Configuration.GetConnectionString("DefaultConnection");


// Xây dựng chuỗi kết nối từ biến môi trường
var dbServer = Environment.GetEnvironmentVariable("DB_SERVER") ?? ".";
var dbName = Environment.GetEnvironmentVariable("DB_NAME") ?? "OHCP_DB";
var dbUser = Environment.GetEnvironmentVariable("DB_USER") ?? "sa";
var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "123456";
var connectionString = $"Server={dbServer};Database={dbName};User Id={dbUser};Password={dbPassword};TrustServerCertificate=True;MultipleActiveResultSets=true";
builder.Services.AddDbContext<OHCPContext>(options =>
    options.UseSqlServer(connectionString)
);

// Đăng ký dịch vụ Authorization
builder.Services.AddAuthorization();

// Cấu hình CORS
builder.Services.AddCors(c =>
{
    c.AddPolicy("AllowReactApp", p =>
    {
        p.WithOrigins("https://ohcp.onrender.com")
         .AllowAnyHeader()
         .AllowAnyMethod()
         .AllowCredentials();
    });
});

// Thêm logging
builder.Services.AddLogging(logging => logging.AddConsole().SetMinimumLevel(LogLevel.Debug));

// Thêm dịch vụ Controllers
builder.Services.AddControllers();

var app = builder.Build();

// Middleware exception handling
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";
        var error = new { Error = "Internal server error. Please check logs." };
        await context.Response.WriteAsJsonAsync(error);
    });
});

// Sử dụng middleware
app.UseCors("AllowReactApp");
app.UseAuthorization();
app.MapControllers();

app.Run();