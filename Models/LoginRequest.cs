namespace WebApiTest.Models
{
    public class LoginRequest
    {
        public int id { get; set; }
        public string? username { get; set; }
        public string? password { get; set; }
        public string? intitule { get; set; }
        public Boolean? enabled { get; set; }
        public string? role { get; set; }
        public string? LocalisationCheckIn { get; set; }
        public string? LocalisationCheckOut { get;set; }
        public DateTime? DateCheckIn { get; set; }
        public DateTime? DateCheckOut { get; set; }

    }
}
