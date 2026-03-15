using Microsoft.AspNetCore.Mvc;
using CloudSRE.API.Data;
using CloudSRE.API.Models;

namespace CloudSRE.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServicesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ServicesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetServices()
        {
            var services = _context.Services.ToList();
            return Ok(services);
        }

        [HttpPost]
        public IActionResult AddService(Service service)
        {
            service.CreatedAt = DateTime.UtcNow;
            service.IsActive = true;

            _context.Services.Add(service);
            _context.SaveChanges();

            return Ok(service);
        }
    }
}