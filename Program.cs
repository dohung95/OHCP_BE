using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OHCP_BK.Data;

var builder = WebApplication.CreateBuilder(args);

// Cấu hình Kestrel chạy HTTP trên port 10000
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(10000); // HTTP, port 10000 cho Render
});

// Xây dựng chuỗi kết nối từ biến môi trường
var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

// Đăng ký DbContext với PostgreSQL (thay vì SQL Server)
builder.Services.AddDbContext<OHCPContext>(options =>
    options.UseNpgsql(connectionString)); // Sử dụng Npgsql cho PostgreSQL

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