using System.Runtime.InteropServices;
using System.Text;

namespace ConfirmScout.Services;

public class WindowPicker
{
    [DllImport("user32.dll")]
    private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll")]
    private static extern int GetWindowTextLength(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    public class WindowInfo
    {
        public IntPtr Handle { get; set; }
        public string Title { get; set; } = string.Empty;
    }

    public static List<WindowInfo> GetAllWindows()
    {
        var windows = new List<WindowInfo>();
        
        EnumWindows((hWnd, lParam) =>
        {
            if (!IsWindowVisible(hWnd))
                return true;

            int length = GetWindowTextLength(hWnd);
            if (length == 0)
                return true;

            var builder = new StringBuilder(length + 1);
            GetWindowText(hWnd, builder, builder.Capacity);
            
            string title = builder.ToString();
            if (!string.IsNullOrWhiteSpace(title))
            {
                windows.Add(new WindowInfo { Handle = hWnd, Title = title });
            }

            return true;
        }, IntPtr.Zero);

        return windows;
    }

    public static IntPtr GetForegroundWindowHandle()
    {
        return GetForegroundWindow();
    }

    public static bool BringWindowToForeground(IntPtr hWnd)
    {
        return SetForegroundWindow(hWnd);
    }
}
