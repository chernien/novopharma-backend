namespace WebApiTest.Models
{
    public class Commande
    {

        public int Id { get; set; }
        public string? PharmacyId { get; set; }
        public string? ArticleId { get; set; }
        public int? Quantity { get; set; }
        public int? QuantityVendue { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime LastUpdated { get; set; }
        public string? CreatedBy { get; set; }
        public bool? isValid { get; set; }
        public decimal? PrixVente { get; set; }
        public string? ImagePath { get; set; }
        public DateTime? DatePeremtion { get; set; }
        public string? NumSerie { get; set; }
        public DateTime? DatePeremtion1 { get; set; }
        public string? NumSerie1 { get; set; }
        public DateTime? DatePeremtion2 { get; set; }
        public string? NumSerie2 { get; set; }
        public int? Qte1 { get; set; }
        public int? Qte2 { get; set; }
        public int? Qte3 { get; set; }
        public string? Source { get; set; }

    }
}
