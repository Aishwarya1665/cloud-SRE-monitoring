using Microsoft.Extensions.Hosting;
using CloudSRE.API.Data;
using CloudSRE.API.Models;
using Microsoft.EntityFrameworkCore;

namespace CloudSRE.API.Services
{
    public class HealthCheckService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly HttpClient _httpClient;

        public HealthCheckService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
            _httpClient = new HttpClient();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var services = await context.Services
                    .Where(s => s.IsActive)
                    .ToListAsync();

                foreach (var service in services)
                {
                    var log = new HealthLog
                    {
                        ServiceId = service.Id,
                        CheckedAt = DateTime.UtcNow
                    };

                    try
                    {
                        var start = DateTime.UtcNow;

                        var response = await _httpClient.GetAsync(service.BaseUrl);

                        var end = DateTime.UtcNow;

                        log.StatusCode = (int)response.StatusCode;
                        log.ResponseTimeMs = (int)(end - start).TotalMilliseconds;
                        log.IsHealthy = response.IsSuccessStatusCode;
                    }
                    catch
                    {
                        log.StatusCode = 0;
                        log.ResponseTimeMs = 0;
                        log.IsHealthy = false;
                    }

                    context.HealthLogs.Add(log);

                    // Check existing active incident
                    var existingIncident = await context.Incidents
                        .FirstOrDefaultAsync(i => i.ServiceId == service.Id && i.Status == "Active");

                    // SERVICE FAILURE
                    if (!log.IsHealthy)
                    {
                        if (existingIncident == null)
                        {
                            var incident = new Incident
                            {
                                ServiceId = service.Id,
                                StartedAt = DateTime.UtcNow,
                                Status = "Active",
                                Description = "Service is down"
                            };

                            context.Incidents.Add(incident);

                            var alert = new Alert
                            {
                                Incident = incident,
                                SentAt = DateTime.UtcNow,
                                Type = "Failure",
                                Email = "admin@monitoring.com"
                            };

                            context.Alerts.Add(alert);
                        }
                    }
                    // SERVICE RECOVERY
                    else
                    {
                        if (existingIncident != null)
                        {
                            existingIncident.Status = "Resolved";
                            existingIncident.ResolvedAt = DateTime.UtcNow;

                            var alert = new Alert
                            {
                                IncidentId = existingIncident.Id,
                                SentAt = DateTime.UtcNow,
                                Type = "Resolved",
                                Email = "admin@monitoring.com"
                            };

                            context.Alerts.Add(alert);
                        }
                    }
                }

                await context.SaveChangesAsync();

                await Task.Delay(30000, stoppingToken);
            }
        }
    }
}676
