using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace URLShortener.Entity
{
    public class UserAgent
    {
        [Key]
        public int ID { get; set; }
        [Column("UserAgent")]
        public string UserAgentString { get; set; }
        public string IPAddress { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
