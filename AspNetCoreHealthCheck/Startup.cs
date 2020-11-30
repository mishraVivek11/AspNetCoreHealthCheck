using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AspNetCoreHealthCheck
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
            string connectionString = this.Configuration["ConnectionStrings:DefaultConnection"];
            string domainServiceUrl = this.Configuration["HealthChecks:DomainServiceUrl"];

            services.AddControllers();
            services.AddHealthChecks()
                .AddSqlServer(
                connectionString,name:"DB Health Check",
                failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy, 
                tags: new string[] { "ready" },
                timeout: new TimeSpan(0, 0, 2)
                )
                .AddUrlGroup(
                new Uri($"{domainServiceUrl}"),
                "Domain Service Health Check",
                failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Degraded,
                 tags: new string[] { "ready" },
                timeout: new TimeSpan(0, 0, 2)
                );
            
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

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/health", new HealthCheckOptions() {
                    ResultStatusCodes = {
                    [HealthStatus.Healthy]=StatusCodes.Status200OK,
                    [HealthStatus.Degraded]=StatusCodes.Status500InternalServerError,
                    [HealthStatus.Unhealthy]=StatusCodes.Status503ServiceUnavailable
                    },
                    AllowCachingResponses=false
                });
            });
        }
    }
}
