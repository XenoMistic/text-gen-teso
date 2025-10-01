namespace UndauntedPledges.Models;

public class DailyPledge
{
    public DateTime DateTime { get; set; }

    public string MajPledge { get; set; } = string.Empty;

    public string GlirionPledge { get; set; } = string.Empty;

    public string UrgarlagPledge { get; set; } = string.Empty;
}