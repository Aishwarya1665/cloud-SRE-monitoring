namespace CloudSRE.API.Models
{
    public class Alert
    {
        public int Id { get; set; }

        public int IncidentId { get; set; }

        public DateTime SentAt { get; set; }

        public string? Type { get; set; }

        public string? Email { get; set; }

        public Incident? Incident { get; set; }
    }
}