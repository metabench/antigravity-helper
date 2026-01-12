using System.Windows;

namespace ConfirmScout.Models;

public class DetectedText
{
    public string Text { get; set; } = string.Empty;
    public Rect BoundingBox { get; set; }
    public double Confidence { get; set; }
    public DateTime DetectedAt { get; set; }
}
