using Microsoft.AspNetCore.HttpLogging;
using Microsoft.EntityFrameworkCore;
using Persistence.Extensions;
using SummerLoginServer.DbContexts;
using SummerLoginServer.Services;
using System.Threading.RateLimiting;

namespace SummerLoginServer;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddHttpLogging(options =>
        {
            options.LoggingFields =
                HttpLoggingFields.RequestMethod |
                HttpLoggingFields.RequestPath |
                HttpLoggingFields.ResponseStatusCode |
                HttpLoggingFields.Duration;
            options.CombineLogs = true;
        });
        builder.Services.AddControllers();
        builder.Services.AddProblemDetails();
        builder.Services.AddHealthChecks();
        builder.Services.AddRateLimiter(options =>
        {
            options.AddPolicy("login", context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    }));
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        });
        builder.Services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, _, _) =>
            {
                document.Info.Title = "SummerLoginServer";
                document.Info.Version = "0.0.1";
                document.Info.Description = "여름방학 비동기 멀티플젝 로그인 서버";
                return Task.CompletedTask;
            });
        });

        string connectionString = builder.Configuration.GetConnectionString("MySql")
            ?? throw new InvalidOperationException("MySql connection string is missing.");
        builder.Services.AddDbContext<UserDbContext>(options =>
            options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 41))));

        builder.Services.AddAppJwtAuthentication(builder.Configuration);
        builder.Services.AddSingleton<JwtTokenService>();
        builder.Services.AddScoped<GoogleService>();

        WebApplication app = builder.Build();

        //if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.UseSwaggerUI(options =>
                options.SwaggerEndpoint("/openapi/v1.json", "SummerLoginServer"));
        }
        app.UseExceptionHandler();
        app.UseHsts();

        app.UseHttpLogging();
        app.UseHttpsRedirection();
        app.UseRateLimiter();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        app.MapHealthChecks("/health");
        app.MapGet("/", () => "SummerLoginServer is running");

        app.Run();
    }
}
