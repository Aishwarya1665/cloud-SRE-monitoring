namespace CloudSRE.API.Models
{
    public class Service
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public string? BaseUrl { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}