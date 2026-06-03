using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebApiTest.Models
{
    public class GiftAssignment
    {

        [Key]
        public int Id { get; set; }

        [Required]
        public string GiftRef { get; set; } // Référence du gift (ArRef)

        [Required]
        public int DermoId { get; set; } // Id du dermo-conseiller

        [Required]
        public decimal QuantiteAttribuee { get; set; } // Quantité affectée

        public DateTime DateAttribution { get; set; } = DateTime.UtcNow;
    }


}
