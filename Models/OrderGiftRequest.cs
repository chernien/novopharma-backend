namespace WebApiTest.Models
{
    public class OrderGiftRequest
    {
        public string GiftRef { get; set; } // Référence du gift
        public int DermoId { get; set; }    // ID du dermo-conseiller
        public decimal QuantiteCommande { get; set; } // Quantité commandée

        // Infos de la pharmacie
        public string CodePharmacie { get; set; }
        public string NomPharmacie { get; set; }

        // ✅ Date de commande facultative (si non fournie → UTCNow)
        public DateTime DateCommande { get; set; }

    }
}
