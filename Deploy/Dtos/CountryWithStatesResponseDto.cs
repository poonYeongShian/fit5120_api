namespace Deploy.Dtos;

public class CountryWithStatesResponseDto
{
    public CountryDto Country { get; set; } = new();
    public List<CountryStateDto> States { get; set; } = [];
}
