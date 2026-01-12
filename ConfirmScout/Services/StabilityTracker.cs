using System.Windows;
using ConfirmScout.Models;

namespace ConfirmScout.Services;

public class StabilityTracker
{
    private readonly Dictionary<string, StableDetection> _detections = new();
    private readonly Logger _logger;
    private const int RequiredFrames = 2;
    private const double PositionTolerance = 10.0; // pixels for X, Y position
    private const double SizeTolerance = 15.0; // pixels for Width, Height dimensions

    public event EventHandler<StableDetection>? StableDetectionFound;

    public StabilityTracker(Logger logger)
    {
        _logger = logger;
    }

    public void ProcessDetections(List<DetectedText> detections)
    {
        var currentTime = DateTime.Now;
        var seenKeys = new HashSet<string>();

        foreach (var detection in detections)
        {
            string key = detection.Text.ToLowerInvariant();
            seenKeys.Add(key);

            if (_detections.TryGetValue(key, out var stable))
            {
                // Check if position is similar
                if (AreBoundingBoxesSimilar(stable.BoundingBox, detection.BoundingBox))
                {
                    stable.FrameCount++;
                    stable.LastSeen = currentTime;
                    stable.BoundingBox = detection.BoundingBox; // Update to latest position

                    // Check if stable now
                    if (stable.FrameCount == RequiredFrames)
                    {
                        _logger.Log($"Stable detection: '{stable.Text}' at {stable.BoundingBox}");
                        StableDetectionFound?.Invoke(this, stable);
                    }
                }
                else
                {
                    // Position changed, reset
                    stable.FrameCount = 1;
                    stable.BoundingBox = detection.BoundingBox;
                    stable.LastSeen = currentTime;
                }
            }
            else
            {
                // New detection
                _detections[key] = new StableDetection
                {
                    Text = detection.Text,
                    BoundingBox = detection.BoundingBox,
                    Confidence = detection.Confidence,
                    FrameCount = 1,
                    LastSeen = currentTime
                };
            }
        }

        // Remove old detections that weren't seen in this frame
        var keysToRemove = _detections.Keys.Where(k => !seenKeys.Contains(k)).ToList();
        foreach (var key in keysToRemove)
        {
            _detections.Remove(key);
        }
    }

    private bool AreBoundingBoxesSimilar(Rect box1, Rect box2)
    {
        // Use separate tolerances for position and size
        return Math.Abs(box1.X - box2.X) < PositionTolerance &&
               Math.Abs(box1.Y - box2.Y) < PositionTolerance &&
               Math.Abs(box1.Width - box2.Width) < SizeTolerance &&
               Math.Abs(box1.Height - box2.Height) < SizeTolerance;
    }

    public StableDetection? GetStableDetection(string text)
    {
        string key = text.ToLowerInvariant();
        if (_detections.TryGetValue(key, out var detection) && detection.FrameCount >= RequiredFrames)
        {
            return detection;
        }
        return null;
    }

    public void Clear()
    {
        _detections.Clear();
    }
}
