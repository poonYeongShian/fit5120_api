namespace Deploy.Models;

public class CountryWithStatesResponse
{
    public Country Country { get; set; } = new();
    public List<CountryState> States { get; set; } = new();
}
