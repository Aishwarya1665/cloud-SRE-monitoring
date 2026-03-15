namespace CloudSRE.API.Models
{
    public class HealthLog
    {
        public int Id { get; set; }

        public int ServiceId { get; set; }

        public int StatusCode { get; set; }

        public int ResponseTimeMs { get; set; }

        public bool IsHealthy { get; set; }

        public DateTime CheckedAt { get; set; }

        public Service? Service { get; set; }
    }
}