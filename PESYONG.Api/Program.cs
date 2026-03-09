using Microsoft.EntityFrameworkCore;
using PESYONG.Api.Hubs;
using PESYONG.ApplicationLogic.Repositories;
using PESYONG.Infrastructure;

namespace PESYONG.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddSignalR();

            builder.Services.AddDbContextFactory<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddScoped<OrderRepository>();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();

            app.MapControllers();
            app.MapHub<PesyongHub>("/hubs/pesyong");

            app.Run();
        }
    }
}