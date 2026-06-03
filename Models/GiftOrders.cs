using System.ComponentModel.DataAnnotations;

namespace WebApiTest.Models
{

    public class GiftOrder
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string GiftRef { get; set; } // Référence du gift

        [Required]
        public int DermoId { get; set; } // ID du dermo

        [Required]
        public decimal QuantiteCommande { get; set; } // Quantité commandée

        [Required]
        public DateTime DateCommande { get; set; } = DateTime.UtcNow; // Date de la commande

        // Infos de la pharmacie
        public string CodePharmacie { get; set; }
        public string NomPharmacie { get; set; }

    }
}
