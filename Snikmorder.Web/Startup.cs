using System.Data.Common;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Snikmorder.Core.Models;
using Snikmorder.Core.Services;
using Telegram.Bot;

namespace SnikmorderTelegramBot
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers().AddNewtonsoftJson();
            
            services.AddHttpClient();

            services.AddScoped<ITelegramBotClient>(provider =>
            {
                var key = Configuration.GetSection("Telegram").GetValue<string>("BotKey");
                return new TelegramBotClient(key, provider.GetService<HttpClient>());
            });

            var builder = new DbConnectionStringBuilder()
            {
                ConnectionString = Configuration.GetConnectionString("CosmosSettings")
            };

            builder.TryGetValue("AccountEndpoint", out object endpoint);
            builder.TryGetValue("AccountKey", out object key);

            services.AddDbContext<GameContext>(option => option.UseCosmos(endpoint?.ToString() ?? "", key?.ToString() ?? "", "snikmorder"));

            services.AddScoped<MessageHandler>();
            services.AddScoped<AdminStateMachine>();
            services.AddScoped<ITelegramSender, TelegramSender>();
            services.AddScoped<PlayerStateMachine>();
            services.AddScoped<GameService>();
            services.AddScoped<IGameRepository, GameRepository>();
            services.AddApplicationInsightsTelemetry();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            _ = UpdateDatabase(app);
        }

        private static async Task UpdateDatabase(IApplicationBuilder app)
        {
            using var serviceScope = app.ApplicationServices
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope();
            await using var context = serviceScope.ServiceProvider.GetService<GameContext>();
            await context.Database.EnsureCreatedAsync();
        }

    }
}
