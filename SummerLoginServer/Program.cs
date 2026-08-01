
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Persistence.Extensions;
using StackExchange.Redis;
using SummerLoginServer.DbContexts;
using SummerLoginServer.Services;
using System.Text;

namespace SummerLoginServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddHttpLogging(options =>
            {
                options.LoggingFields =
                    HttpLoggingFields.RequestMethod |
                    HttpLoggingFields.RequestPath |
                    HttpLoggingFields.RequestQuery |
                    HttpLoggingFields.ResponseStatusCode |
                    HttpLoggingFields.Duration;

                // 요청과 응답 정보를 한 로그로 합침
                options.CombineLogs = true;
            });
            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi(options =>
            {
                options.AddDocumentTransformer((document, context, ct) =>
                {
                    document.Info.Title = "SummerLoginServer";
                    document.Info.Version = "0.0.1";
                    document.Info.Description = "여름방학 비동기 멀티플젝 로그인 서버";
                    return Task.CompletedTask;
                });
            });

            string? connectionString = builder.Configuration.GetConnectionString("MySql")
                ?? throw new InvalidOperationException("Connection string MySql not found");
            builder.Services.AddDbContext<UserDbContext>(options =>
            {
                options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 41)));
            });
            //string? redisConnection = builder.Configuration.GetConnectionString("Redis")
            //    ?? throw new InvalidOperationException("Connection string MySql not found");

            builder.Services.AddAppJwtAuthentication(builder.Configuration);

            //builder.Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnection));
            builder.Services.AddSingleton<JwtTokenService>();
            builder.Services.AddScoped<GoogleService>();
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            //if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/openapi/v1.json", "SummerLoginServer");
                });
            }
            app.UseHttpLogging();
            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();
            app.MapGet("/", () => "SummerLoginServer is running");

            app.Run();
        }
    }
}
