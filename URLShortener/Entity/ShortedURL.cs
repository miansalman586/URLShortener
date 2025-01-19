using System.ComponentModel.DataAnnotations;

namespace URLShortener.Entity
{
    public class ShortedURL
    {
        [Key]
        public int ID { get; set; }
        public string LongURL { get; set; }
        public DateTime CreatedDate { get; set; }
        public int View { get; set; }
        public DateTime? LastView { get; set; }
        public string? CustomizedURL { get; set; }
        public bool? IsDeleted { get; set; }
    }
}
