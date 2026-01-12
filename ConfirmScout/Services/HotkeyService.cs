using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace ConfirmScout.Services;

public class HotkeyService : IDisposable
{
    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    private const int WM_HOTKEY = 0x0312;
    private const int HOTKEY_CLICK_ID = 1;
    private const int HOTKEY_SCROLL_ID = 2;

    // Modifiers
    private const uint MOD_ALT = 0x0001;
    private const uint MOD_CONTROL = 0x0002;

    // Virtual keys
    private const uint VK_RETURN = 0x0D; // Enter
    private const uint VK_S = 0x53;

    private readonly IntPtr _windowHandle;
    private readonly HwndSource _hwndSource;
    private readonly Logger _logger;

    public event EventHandler? ClickHotkeyPressed;
    public event EventHandler? ScrollHotkeyPressed;

    public HotkeyService(IntPtr windowHandle, Logger logger)
    {
        _windowHandle = windowHandle;
        _logger = logger;

        _hwndSource = HwndSource.FromHwnd(_windowHandle);
        _hwndSource.AddHook(WndProc);

        RegisterHotkeys();
    }

    private void RegisterHotkeys()
    {
        // Register Ctrl+Alt+Enter
        if (!RegisterHotKey(_windowHandle, HOTKEY_CLICK_ID, MOD_CONTROL | MOD_ALT, VK_RETURN))
        {
            _logger.Log("Failed to register Ctrl+Alt+Enter hotkey");
        }
        else
        {
            _logger.Log("Registered Ctrl+Alt+Enter hotkey");
        }

        // Register Ctrl+Alt+S
        if (!RegisterHotKey(_windowHandle, HOTKEY_SCROLL_ID, MOD_CONTROL | MOD_ALT, VK_S))
        {
            _logger.Log("Failed to register Ctrl+Alt+S hotkey");
        }
        else
        {
            _logger.Log("Registered Ctrl+Alt+S hotkey");
        }
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WM_HOTKEY)
        {
            int id = wParam.ToInt32();
            
            if (id == HOTKEY_CLICK_ID)
            {
                _logger.Log("Ctrl+Alt+Enter pressed");
                ClickHotkeyPressed?.Invoke(this, EventArgs.Empty);
                handled = true;
            }
            else if (id == HOTKEY_SCROLL_ID)
            {
                _logger.Log("Ctrl+Alt+S pressed");
                ScrollHotkeyPressed?.Invoke(this, EventArgs.Empty);
                handled = true;
            }
        }

        return IntPtr.Zero;
    }

    public void Dispose()
    {
        UnregisterHotKey(_windowHandle, HOTKEY_CLICK_ID);
        UnregisterHotKey(_windowHandle, HOTKEY_SCROLL_ID);
        _hwndSource.RemoveHook(WndProc);
        _logger.Log("Hotkeys unregistered");
        GC.SuppressFinalize(this);
    }
}
