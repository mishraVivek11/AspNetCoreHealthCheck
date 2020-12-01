# AspNetCoreHealthCheck
Exploring Asp.NetCore HealthChecks middleware.

Solution contains 2 Web API projects.

AspNetCoreHealthCheck : Web API project for testing and validating health checks, including
1) Alive and ready health checks using tags.
2) DB health checks.
3) Custom health check extension to check for a certificate
4) URL group healthChecks (dependant services)
5) Custom health check response

DomainService.WebAPI : Web API project hosting a dependant service for validating URL group healthChecks.
Implements a basic health check endpoint with CORS.



