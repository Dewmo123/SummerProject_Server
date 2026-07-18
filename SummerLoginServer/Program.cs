
using Microsoft.EntityFrameworkCore;
using SummerLoginServer.DbContexts;

namespace SummerLoginServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

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
            
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/openapi/v1.json", "SummerLoginServer");
                });
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();
            app.MapGet("/", () => "SummerLoginServer is running");

            app.Run();
        }
    }
}
