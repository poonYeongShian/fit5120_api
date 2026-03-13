namespace Deploy.Dtos;

public class CountryStateDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<MonthlyAqiDto> MonthlyAqi { get; set; } = [];
}
