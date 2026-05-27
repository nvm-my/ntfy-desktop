namespace NtfyDesktop.Domain;

public enum Priority
{
    Min = 1,
    Low = 2,
    Default = 3,
    High = 4,
    Urgent = 5
}

public static class PriorityExtensions
{
    extension(Priority p)
    {
        public string Label => p switch
        {
            Priority.Min => "Min",
            Priority.Low => "Low",
            Priority.Default => "Default",
            Priority.High => "High",
            Priority.Urgent => "Urgent",
            _ => p.ToString()
        };
    }
}
