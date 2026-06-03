using System;

namespace WebApiTest.Models
{
    public class CommandeViewModelDto2
    {
        public int Id { get; set; }
        public string? PharmacyId { get; set; }     // ajouté
        public string? ArticleId { get; set; }      // ajouté
        public object pharmacy { get; set; }
        public object article { get; set; }
        public int? Quantity { get; set; }
        public int? QuantityVendue { get; set; }
        public DateTime? DateCreated { get; set; }  // nullable
        //public DateTime? LastUpdated { get; set; }
        public string? CreatedBy { get; set; }
        //public bool? isValid { get; set; }
        public decimal? PrixVente { get; set; }
        public string? ImagePath { get; set; }
        public DateTime? DatePeremtion { get; set; }
        //public string? NumSerie { get; set; }
        public DateTime? DatePeremtion1 { get; set; }
        //public string? NumSerie1 { get; set; }
        public DateTime? DatePeremtion2 { get; set; }
        //public string? NumSerie2 { get; set; }
        public int? Qte1 { get; set; }
        public int? Qte2 { get; set; }
        public int? Qte3 { get; set; }

        // Champs liés à la facture
        public int? FactureId { get; set; }
        public string? CodePharmacie { get; set; }
        public DateTime? DateArrivee { get; set; }
    }
}
