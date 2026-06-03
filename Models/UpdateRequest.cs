public class UpdateLocalisationRequest
{
    public string username { get; set; }
    public string? LocalisationCheckIn { get; set; }
    public DateTime? DateCheckIn { get; set; }
    public string? LocalisationCheckOut { get; set; }
    public DateTime? DateCheckOut { get; set; }
}
