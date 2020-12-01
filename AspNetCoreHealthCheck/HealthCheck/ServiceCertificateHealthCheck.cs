using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCoreHealthCheck.HealthCheck
{
    public class ServiceCertificateHealthCheck : IHealthCheck
    {
        private string certFilePath;

        public ServiceCertificateHealthCheck(string certFilePath)
        {
            this.certFilePath = certFilePath;
        }
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try 
            {
                if (File.Exists(this.certFilePath))
                    return Task.FromResult<HealthCheckResult>(HealthCheckResult.Healthy());

                throw new FileNotFoundException(this.certFilePath);
            }
            catch (Exception ex) 
            { 
                switch(context.Registration.FailureStatus)
                {
                    case HealthStatus.Healthy:
                        return Task.FromResult<HealthCheckResult>(new HealthCheckResult(HealthStatus.Healthy, description: "Certificate file not found"));

                    default:
                        return Task.FromResult<HealthCheckResult>(new HealthCheckResult(context.Registration.FailureStatus, description: "Certificate file not found.", exception: ex));
                }
            }
        }
    }
}
