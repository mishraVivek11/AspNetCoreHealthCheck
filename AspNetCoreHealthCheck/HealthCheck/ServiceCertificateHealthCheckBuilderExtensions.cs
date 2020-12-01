using AspNetCoreHealthCheck.HealthCheck;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCertificateHealthCheckBuilderExtensions
    {
        public static IHealthChecksBuilder AddServiceCertificateCheck(this IHealthChecksBuilder builder, string certificatePath, HealthStatus healthStatus, IEnumerable<string> tags = null, TimeSpan? timeout = null)
        {
            if(string.IsNullOrEmpty(certificatePath))
                throw new ArgumentNullException(nameof(certificatePath));

           return builder.AddCheck("Service Certificate Health Check", new ServiceCertificateHealthCheck(certificatePath), healthStatus, tags, timeout);


        }
    }
}
