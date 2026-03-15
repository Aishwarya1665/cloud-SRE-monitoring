namespace CloudSRE.API.Models
{
    public class ServiceUptimeDto
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = "";
        public double UptimePercentage { get; set; }
    }
}