namespace Deploy.Dtos;

public class MonthlyAqiDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public double AvgAqi { get; set; }
    public double PeakAqi { get; set; }
    public int WorsenigDays { get; set; }
    public int ImprovingDays { get; set; }
}
