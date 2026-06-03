namespace WebApiTest.Models
{
    public class CommandeViewModel
    {
        public MsAClients pharmacy { get; set; }
        public MsAArticle article { get; set; }
        public int Quantity { get; set; }
        public int QuantityVendue { get; set; }
        public DateTime? DatePeremtion { get; set; }
        public DateTime Datecreated { get; set; }
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
