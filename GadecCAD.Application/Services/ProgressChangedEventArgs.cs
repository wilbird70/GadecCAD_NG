namespace GadecCAD.Application.Services;
public class FrameSetProgressEventArgs : EventArgs
{
    public int CurrentValue { get; set; }
    public int MaxValue { get; set; }
    public string? Message { get; set; }

    public FrameSetProgressEventArgs(int currentValue, int maxValue, string? message)
    {
        CurrentValue = currentValue;
        MaxValue = maxValue;
        Message = message;
    }
}
