using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Diagnostics;
using System.Net;
using System.Security.Principal;

namespace NavisSmartRestGateway.RestIntrastructure
{
    public class RestStartup
    {
        private IConfiguration _configuration;
        private HttpListener _httpListener;

        public RestStartup(IConfiguration inConfiguration)
        {
            _configuration = inConfiguration;
            _httpListener = new HttpListener();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }


            app.UseCors(
                        options => options.WithOrigins("http://localhost:3000").AllowAnyMethod()
                );

            app.UseHttpsRedirection();


            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
