using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CloudSRE.API.Data;
using CloudSRE.API.Models;

namespace CloudSRE.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/dashboard/uptime
        [HttpGet("uptime")]
        public async Task<IActionResult> GetServiceUptime()
        {
            var services = await _context.Services.ToListAsync();
            var logs = await _context.HealthLogs.ToListAsync();

            var result = new List<ServiceUptimeDto>();

            foreach (var service in services)
            {
                var serviceLogs = logs.Where(l => l.ServiceId == service.Id).ToList();

                if (!serviceLogs.Any())
                    continue;

                var healthy = serviceLogs.Count(l => l.IsHealthy);
                double uptime = (double)healthy / serviceLogs.Count * 100;

                result.Add(new ServiceUptimeDto
                {
                    ServiceId = service.Id,
                    ServiceName = service.Name,
                    UptimePercentage = Math.Round(uptime, 2)
                });
            }

            return Ok(result);
        }

        // GET: api/dashboard/summary
        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            var totalServices = await _context.Services.CountAsync();

            var activeIncidents = await _context.Incidents
                .CountAsync(i => i.Status == "Active");

            var healthyServices = await _context.HealthLogs
                .Where(h => h.IsHealthy)
                .Select(h => h.ServiceId)
                .Distinct()
                .CountAsync();

            var logs = await _context.HealthLogs.ToListAsync();

            double avgUptime = 0;

            if (logs.Any())
            {
                var healthyLogs = logs.Count(l => l.IsHealthy);
                avgUptime = (double)healthyLogs / logs.Count * 100;
            }

            return Ok(new
            {
                totalServices,
                healthyServices,
                activeIncidents,
                averageUptime = Math.Round(avgUptime, 2)
            });
        }

        // GET: api/dashboard/status
        [HttpGet("status")]
        public async Task<IActionResult> GetServiceStatus()
        {
            var services = await _context.Services.ToListAsync();
            var logs = await _context.HealthLogs
                .OrderByDescending(h => h.CheckedAt)
                .ToListAsync();

            var result = new List<object>();

            foreach (var service in services)
            {
                var lastLog = logs.FirstOrDefault(l => l.ServiceId == service.Id);

                string status = "Unknown";

                if (lastLog != null)
                {
                    if (!lastLog.IsHealthy)
                        status = "Down";
                    else if (lastLog.ResponseTimeMs > 1000)
                        status = "Slow";
                    else
                        status = "Healthy";
                }

                result.Add(new
                {
                    service.Id,
                    service.Name,
                    Status = status,
                    LastChecked = lastLog?.CheckedAt,
                    ResponseTime = lastLog?.ResponseTimeMs
                });
            }

            return Ok(result);
        }

        // GET: api/dashboard/incidents
        [HttpGet("incidents")]
        public async Task<IActionResult> GetIncidents()
        {
            var incidents = await _context.Incidents
                .Include(i => i.Service) // join service table
                .OrderByDescending(i => i.CreatedAt)
                .Select(i => new
                {
                    i.Id,
                    ServiceName = i.Service.Name,
                    i.Description,
                    i.Severity,
                    i.Status,
                    i.CreatedAt
                })
                .ToListAsync();

            return Ok(incidents);
        }
    }
}