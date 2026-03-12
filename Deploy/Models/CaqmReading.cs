namespace Deploy.Models;

public class CaqmReading
{
    public long Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string Payload { get; set; } = string.Empty;
}
