
using GIS.City.Service.Entities;
using GIS.City.Service.Services;
using GIS.Common.Repositories;

namespace GIS.City.Service
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configure logging settings

            builder.Logging.AddConsole();
            builder.Logging.AddDebug();

            // Add services to the container.

            builder.Services.AddMongoDatabase()
                .AddMongoRepository<CityEntity>("info");

            builder.Services.AddAuthenticationSettings();

            builder.Services.AddProblemDetails();

            builder.Services.AddHttpContextAccessor();

            builder.Services.AddTransient<TokenHandler>();

            builder.Services.AddHttpClient<ICityService, CityService>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:44330/");
                
            }).AddHttpMessageHandler<TokenHandler>();

            builder.Services.AddHttpClient<IDistrictService, DistrictService>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:44333/");

            }).AddHttpMessageHandler<TokenHandler>();

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

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

            //app.UseMiddleware<CustomExceptionHandlerMiddleware>();

            app.UseExceptionHandler();

            app.MapControllers();

            app.Run();
        }
    }
}
