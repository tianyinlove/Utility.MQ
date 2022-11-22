using Emapp.Configuration.Model;
using Emapp.Constants;
using Emapp.Extensions;
using Utility.MQ.Configuration;
using Utility.MQ.Workers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Utility.MQ
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
            services.AddHttpContextAccessor();
            services.AddEmappApi();
            services.AddEmappApiClient();
            services.AddEmappLog();
            services.AddEmappJob();
            services.AddEmappService();
            services.AddEmappContext();
            services.AddEmappConfig();

            services.Configure<AppSettings>(Configuration.GetSection($"{ConfigSections.AppSettings}:MQ"));
            services.AddHostedService<EmappFailedMessageLogService>();
            services.AddHostedService<ClassicFailedMessageLogService>();
            services.AddHostedService<EmappWrongMessageLogService>();
            services.AddHostedService<ClassicWrongMessageLogService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.EnsureEmappDependency();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseEmappException();
            }
            app.UseEmappApi();
        }
    }
}
