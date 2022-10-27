using System.ComponentModel.DataAnnotations.Schema;

namespace quizzish.Models
{//frågortabellen
    public class fragor
    {
        public int Id { get; set; }
        [ForeignKey("svarid")]
        public int svarid { get; set; }
        public virtual svar svar { get; set; }
        public string fraga { get; set; }
        public string svarkat { get; set; }
        public string kat { get; set; }
    }
}
