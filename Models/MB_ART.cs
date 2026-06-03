namespace WebApiTest.Models
{
    public class MB_ART
    {
        public string AR_Ref { get; set; } // Référence de l'article
        public short? AR_SuiviStock { get; set; } // Suivi du stock
        public string? LS_NoSerie { get; set; } // Numéro de série
        public DateTime? LS_Peremption { get; set; } // Date de péremption
        public string? AR_Design { get; set; } // Désignation
        public decimal AR_PUNet { get; set; } // Prix unitaire net
        public string TypeSuivi { get; set; } // Type de suivi

        public string? AR_CodeBarre { get; set; }
    }
}
