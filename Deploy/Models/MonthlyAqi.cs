namespace Deploy.Models;

public class MonthlyAqi
{
    public int StateId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public double AverageAqiValue { get; set; }
    public double PeakAqiValue { get; set; }
    public int WorsenigDays { get; set; }
    public int ImprovingDays { get; set; }
}
