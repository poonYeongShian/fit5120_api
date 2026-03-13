namespace Deploy.Models;

public class Country
{
    public int Id { get; set; }
    public string CountryName { get; set; } = string.Empty;
    public string Iso2Code { get; set; } = string.Empty;
    public string Iso3Code { get; set; } = string.Empty;
    public string? Continent { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public string? CurrencySymbol { get; set; }
    public string PhoneCode { get; set; } = string.Empty;
    public string? CountryDomain { get; set; }
    public string? Nationality { get; set; }
    public string? CurrencyName { get; set; }
}
