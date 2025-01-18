using System.ComponentModel.DataAnnotations;

namespace URLShortener.Entity
{
    public class Contact
    {
        [Key]
        public int ID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
