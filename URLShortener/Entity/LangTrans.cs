using System.ComponentModel.DataAnnotations;

namespace URLShortener.Entity
{
    public class LangTrans
    {
        [Key]
        public int ID { get; set; }
        public string LangCode { get; set; }
        public string TextCode { get; set; }
        public string Text { get; set; }
        public string? Translation { get; set; }
    }
}
