using System.Runtime.InteropServices;
using System.Windows;

namespace ConfirmScout.Services;

public class MouseController
{
    [DllImport("user32.dll")]
    private static extern bool SetCursorPos(int x, int y);

    [DllImport("user32.dll")]
    private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, int dwExtraInfo);

    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out POINT lpPoint);

    [DllImport("user32.dll")]
    private static extern bool ClientToScreen(IntPtr hWnd, ref POINT lpPoint);

    [DllImport("user32.dll")]
    private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int X;
        public int Y;
    }

    private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
    private const uint MOUSEEVENTF_LEFTUP = 0x0004;
    private const uint WM_VSCROLL = 0x0115;
    private const uint SB_LINEDOWN = 1;

    private readonly Logger _logger;
    private DateTime _lastClickTime = DateTime.MinValue;
    private readonly TimeSpan _clickCooldown = TimeSpan.FromSeconds(1);

    public MouseController(Logger logger)
    {
        _logger = logger;
    }

    public void MoveCursor(IntPtr windowHandle, Rect boundingBox)
    {
        try
        {
            // Calculate center of bounding box
            int centerX = (int)(boundingBox.X + boundingBox.Width / 2);
            int centerY = (int)(boundingBox.Y + boundingBox.Height / 2);

            // Convert client coordinates to screen coordinates
            var point = new POINT { X = centerX, Y = centerY };
            ClientToScreen(windowHandle, ref point);

            SetCursorPos(point.X, point.Y);
            _logger.LogAction($"Moved cursor to ({point.X}, {point.Y})");
        }
        catch (Exception ex)
        {
            _logger.Log($"Move cursor error: {ex.Message}");
        }
    }

    public bool ClickAtPosition(IntPtr windowHandle, Rect boundingBox)
    {
        // Check cooldown
        if (DateTime.Now - _lastClickTime < _clickCooldown)
        {
            _logger.Log("Click blocked due to cooldown");
            return false;
        }

        try
        {
            // Calculate center of bounding box
            int centerX = (int)(boundingBox.X + boundingBox.Width / 2);
            int centerY = (int)(boundingBox.Y + boundingBox.Height / 2);

            // Convert client coordinates to screen coordinates
            var point = new POINT { X = centerX, Y = centerY };
            ClientToScreen(windowHandle, ref point);

            // Move cursor and click
            SetCursorPos(point.X, point.Y);
            System.Threading.Thread.Sleep(50); // Small delay

            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
            System.Threading.Thread.Sleep(50);
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);

            _lastClickTime = DateTime.Now;
            _logger.LogAction($"Clicked at ({point.X}, {point.Y})");
            return true;
        }
        catch (Exception ex)
        {
            _logger.Log($"Click error: {ex.Message}");
            return false;
        }
    }

    public void ScrollDown(IntPtr windowHandle)
    {
        try
        {
            SendMessage(windowHandle, WM_VSCROLL, (IntPtr)SB_LINEDOWN, IntPtr.Zero);
            _logger.LogAction("Scrolled down");
        }
        catch (Exception ex)
        {
            _logger.Log($"Scroll error: {ex.Message}");
        }
    }
}
