using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
            services.AddControllers();
            
            services.AddSingleton<PlayerRepository>();

            services.AddHttpClient();

            services.AddScoped<ITelegramBotClient>(provider =>
            {
                var key = Configuration.GetSection("Telegram").GetValue<string>("BotKey");
                return new TelegramBotClient(key, provider.GetService<HttpClient>());
            });

            services.AddScoped<MessageHandler>();
            services.AddScoped<AdminService>();
            services.AddScoped<TelegramSender>();
            services.AddScoped<PlayerStateMachine>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
