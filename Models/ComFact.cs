using System;

namespace WebApiTest.Models
{
    public class ComFact
    {
        public int Id { get; set; }
        public string? PharmacyId { get; set; }
        public string? ArticleId { get; set; }
        public int? Quantity { get; set; }
        public int? QuantityVendue { get; set; }
        public DateTime? DateCreated { get; set; }
        //public DateTime? LastUpdated { get; set; }
        public string? CreatedBy { get; set; }
        //public bool? isValid { get; set; }
        public decimal? PrixVente { get; set; }
        public string? ImagePath { get; set; }
        public DateTime? DatePérumption { get; set; }
        //public string? NumSérie { get; set; }
        public DateTime? DatePérumption1 { get; set; }
        public DateTime? DatePérumption2 { get; set; }
       // public string? NumSérie1 { get; set; }
       // public string? NumSérie2 { get; set; }
        public int? Qte1 { get; set; }
        public int? Qte2 { get; set; }
        public int? Qte3 { get; set; }
        public string? Source { get; set; }

        // Champs de la facture
        public int? FactureId { get; set; }
        public string? code_pharmacie { get; set; }
        public DateTime? DateArrivee { get; set; }
    }
}
