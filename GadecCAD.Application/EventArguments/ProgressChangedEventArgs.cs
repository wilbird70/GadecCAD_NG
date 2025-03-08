namespace GadecCAD.Application.EventArguments;
public class ProgressChangedEventArgs : EventArgs
{
    public int CurrentValue { get; set; }
    public int MaxValue { get; set; }
    public string? Message { get; set; }

    public ProgressChangedEventArgs(int currentValue, int maxValue, string? message)
    {
        CurrentValue = currentValue;
        MaxValue = maxValue;
        Message = message;
    }
}
