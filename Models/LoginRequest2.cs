namespace WebApiTest.Models
{
    public class LoginRequest2
    {
        public int id { get; set; }
        public string? username { get; set; }
        public string? password { get; set; }
        public string? intitule { get; set; }
        public Boolean? enabled { get; set; }
        public string? role { get; set; }
    }
}
