using System.ComponentModel.DataAnnotations;

namespace WebApiTest.Models
{
    public class Facture
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(255)]
        public string Nom { get; set; }

        [MaxLength(500)]
        public string? Path { get; set; }

        [MaxLength(50)]
        public string CodePharmacie { get; set; }

        [MaxLength(255)]
        public string NomPharmacie { get; set; }

        [MaxLength(255)]
        public string NomDermo { get; set; }

        public DateTime? DateFacture { get; set; }
        public DateTime? DateArrivee { get; set; }
        public DateTime? DateSortie { get; set; }
        public string? Commentaire { get; set; }

    }
}
