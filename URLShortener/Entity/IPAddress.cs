using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace URLShortener.Entity
{
    public class IPAddress
    {
        [Key]
        public int ID { get; set; }
        [Column("IPAddress")]
        public string IPAddressString { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
