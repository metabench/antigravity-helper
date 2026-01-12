using System.IO;

namespace ConfirmScout.Services;

public class Logger
{
    private static readonly object _lock = new object();
    private readonly string _logFilePath;

    public Logger()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var logDir = Path.Combine(appData, "ConfirmScout", "Logs");
        Directory.CreateDirectory(logDir);
        _logFilePath = Path.Combine(logDir, $"log_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
    }

    public void Log(string message)
    {
        var logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}";
        
        lock (_lock)
        {
            File.AppendAllText(_logFilePath, logMessage + Environment.NewLine);
        }
        
        System.Diagnostics.Debug.WriteLine(logMessage);
    }

    public void LogDetection(string text, double confidence)
    {
        Log($"Detection: '{text}' (Confidence: {confidence:F2})");
    }

    public void LogAction(string action)
    {
        Log($"Action: {action}");
    }
}
