namespace Deploy.Models;

public class CountryState
{
    public int Id { get; set; }
    public string StateName { get; set; } = string.Empty;
    public int CountryId { get; set; }
    public string? StateCode { get; set; }
}
