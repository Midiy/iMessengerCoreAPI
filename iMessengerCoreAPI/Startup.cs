using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

using iMessengerCoreAPI.Models;

namespace iMessengerCoreAPI
{
#pragma warning disable CS1591 // Отсутствует комментарий XML для открытого видимого типа или члена
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
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "iMessengerCoreAPI", Version = "v1" });

                string xmlDocPath = Path.Combine(AppContext.BaseDirectory, "iMessengerCoreAPI.xml");
                c.IncludeXmlComments(xmlDocPath);
            });

            // RGDialogsClients is expected to provide a general source of information about dialogs,
            // so it is added as a singleton.
            services.AddSingleton<RGDialogsClients>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "iMessengerCoreAPI v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
#pragma warning restore CS1591 // Отсутствует комментарий XML для открытого видимого типа или члена
}
