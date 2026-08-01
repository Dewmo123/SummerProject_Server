using Microsoft.EntityFrameworkCore;
using Persistence.Extensions;
using SummerGameServer.DbContexts;
using SummerGameServer.Services;
using System.ComponentModel.DataAnnotations;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, ct) =>
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
builder.Services.AddAppJwtAuthentication(builder.Configuration);
string mySqlConnection = builder.Configuration.GetConnectionString("MySql") ?? throw new Exception("MySql ConnectionString is null");
builder.Services.AddDbContext<UserDbContext>(options => options.UseMySql(mySqlConnection, new MySqlServerVersion(new Version(8, 0, 41))));
var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "SummerGameServer");
    });
}
app.MapControllers();
app.MapGet("/", () => "SummerGamenServer is running");
app.UseHttpsRedirection();

app.Run();
