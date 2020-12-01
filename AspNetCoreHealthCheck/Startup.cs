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
using Newtonsoft.Json.Linq;

namespace AspNetCoreHealthCheck
{
    public class Startup
    {
        public Startup(IConfiguration configuration,IWebHostEnvironment webHostEnvironment)
        {
            Configuration = configuration;
            WebHostEnvironment = webHostEnvironment;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment WebHostEnvironment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            ConfigureHealthChecks(services);
        }

        private void ConfigureHealthChecks(IServiceCollection services)
        {
            string connectionString = this.Configuration["ConnectionStrings:DefaultConnection"];
            string domainServiceUrl = this.Configuration["HealthChecks:DomainServiceUrl"];
            string certificatePath = this.Configuration["HealthChecks:CertificatePath"];

            services.AddHealthChecks()
               .AddSqlServer(
               connectionString, name: "DB Health Check",
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
               )
               //Custom health check for certificate file
               .AddServiceCertificateCheck(
                WebHostEnvironment.ContentRootPath+ certificatePath,
                healthStatus: HealthStatus.Unhealthy,
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
                // health check for a basic liveness check
                endpoints.MapHealthChecks("/health", new HealthCheckOptions()
                {
                    ResponseWriter = HealthCheckResponseWriter,
                    AllowCachingResponses = false,
                    //excludes other health checks and returns 200
                    Predicate = (_) => false
                });
                //health check for readiness checks
                endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions()
                {
                    ResultStatusCodes = {
                    [HealthStatus.Healthy]=StatusCodes.Status200OK,
                    [HealthStatus.Degraded]=StatusCodes.Status500InternalServerError,
                    [HealthStatus.Unhealthy]=StatusCodes.Status503ServiceUnavailable
                    },
                    ResponseWriter = HealthCheckResponseWriter,
                    AllowCachingResponses = false,
                    Predicate = (healthCheckReg) => healthCheckReg.Tags.Contains("ready")
                });

            });
        }

        private Task HealthCheckResponseWriter(HttpContext context, HealthReport rpt)
        {
            context.Response.ContentType = "application/json";

            var json = new JObject
                (
                    new JProperty("Status", rpt.Status.ToString()),
                    new JProperty("CheckDuration", rpt.TotalDuration.TotalSeconds.ToString("0.000")),
                    new JProperty("Details", new JObject(rpt.Entries.Select(dictItem =>
                 new JProperty(dictItem.Key, new JObject(
                     new JProperty("Status", dictItem.Value.Status.ToString()),
                     new JProperty("CheckDuration", dictItem.Value.Duration.TotalSeconds.ToString("0.000"))
                     ))
             )))
                );
            return context.Response.WriteAsync(json.ToString(Newtonsoft.Json.Formatting.Indented));
        }
    }
}
