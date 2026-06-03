namespace WebApiTest.Models
{
    public class F_Lotserie
    {
        public string AR_Ref { get; set; }
        public string LS_NoSerie { get; set; }
        public DateTime? LS_Peremption { get; set; }
        public DateTime? LS_Fabrication { get; set; }
        public decimal LS_Qte { get; set; }
        public decimal LS_QteRestant { get; set; }
        public decimal LS_QteRes { get; set; }
    }
}
