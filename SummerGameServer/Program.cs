using Microsoft.EntityFrameworkCore;
using Persistence.Extensions;
using SummerGameServer.DbContexts;
using SummerGameServer.Services;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, _, _) =>
    {
        document.Info.Title = "SummerGameServer";
        document.Info.Version = "0.0.1";
        document.Info.Description = "여름방학 비동기 멀티플젝 게임 서버";
        return Task.CompletedTask;
    });
});

builder.Services.AddScoped<StageService>();
builder.Services.AddScoped<CharacterService>();
builder.Services.AddScoped<CurrencyService>();
builder.Services.AddSingleton(CatalogManager.LoadFrom(builder.Environment.ContentRootPath));
builder.Services.AddControllers();

builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.User.FindFirst("sub")?.Value ??
            context.Connection.RemoteIpAddress?.ToString() ??
            "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 120,
                Window = TimeSpan.FromSeconds(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});
builder.Services.AddAppJwtAuthentication(builder.Configuration);

string mySqlConnection = builder.Configuration.GetConnectionString("MySql")
    ?? throw new InvalidOperationException("MySql connection string is missing.");
builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseMySql(mySqlConnection, new MySqlServerVersion(new Version(8, 0, 41))));

var app = builder.Build();

//if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
        options.SwaggerEndpoint("/openapi/v1.json", "SummerGameServer"));
}
app.UseExceptionHandler();
app.UseHsts();

app.UseHttpsRedirection();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");
app.MapGet("/", () => "SummerGameServer is running");

app.Run();

public partial class Program;
