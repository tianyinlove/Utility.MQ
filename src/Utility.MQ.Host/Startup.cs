using Utility.RabbitMQ.Configuration;
using Utility.RabbitMQ.Workers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Utility.Extensions;

namespace Utility.RabbitMQ
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
            services.AddMQService();
            services.Configure<AppSettings>(Configuration); //�Զ�����������
            services.AddHostedService<EmappFailedMessageLogService>();
            services.AddHostedService<ClassicFailedMessageLogService>();
            services.AddHostedService<EmappWrongMessageLogService>();
            services.AddHostedService<ClassicWrongMessageLogService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
        }
    }
}
