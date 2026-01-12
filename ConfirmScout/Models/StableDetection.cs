using System.Windows;

namespace ConfirmScout.Models;

public class StableDetection
{
    public string Text { get; set; } = string.Empty;
    public Rect BoundingBox { get; set; }
    public double Confidence { get; set; }
    public int FrameCount { get; set; }
    public DateTime LastSeen { get; set; }
}
