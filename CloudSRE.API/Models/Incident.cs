namespace CloudSRE.API.Models
{
    public class Incident
    {
        public int Id { get; set; }

        public int ServiceId { get; set; }

        public DateTime StartedAt { get; set; }

        public DateTime? ResolvedAt { get; set; }

        public string Status { get; set; } = "";

        public string Description { get; set; } = "";

        public Service? Service { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }
}