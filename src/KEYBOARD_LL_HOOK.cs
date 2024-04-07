#if WINDOWS && USE_FORMS
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using jammer;

public sealed class KeyboardHook : IDisposable
{
    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN = 0x0100;
    private const int WM_SYSKEYDOWN = 0x0104;

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);
    /*
    [DllImport("user32.dll")]
    static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
    [DllImport("user32.dll")]
    static extern int GetWindowTextLength(IntPtr hWnd);

    [DllImport("user32.dll")]
    static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder text, int count);
    */

    private LowLevelKeyboardProc _proc;
    private IntPtr _hookID = IntPtr.Zero;

    public KeyboardHook()
    {
        _proc = HookCallback;
    }

    public void HookKeyboard()
    {
        _hookID = SetHook(_proc);
    }

    public void UnhookKeyboard()
    {
        UnhookWindowsHookEx(_hookID);
    }

    private IntPtr SetHook(LowLevelKeyboardProc proc)
    {
        using (var curProcess = System.Diagnostics.Process.GetCurrentProcess())
        using (var curModule = curProcess.MainModule)
        {
            if(curModule != null ) return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            return IntPtr.Zero;
        }
    }

    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN))
        {
            int vkCode = Marshal.ReadInt32(lParam);
            // System.Diagnostics.Debug.WriteLine((Keys)vkCode);

            // Check if the pressed key is a media key
            /* 
            if ((Keys)vkCode == Keys.MediaNextTrack || (Keys)vkCode == Keys.MediaPreviousTrack || 
            (Keys)vkCode == Keys.MediaPlayPause || (Keys)vkCode == Keys.MediaStop)
            {
                // Get a handle to the foreground window
                IntPtr foregroundWindow = GetForegroundWindow();

                // Get the title of the foreground window
                int length = GetWindowTextLength(foregroundWindow);
                System.Text.StringBuilder foregroundWindowText = new(length + 1);
                    _ = GetWindowText(foregroundWindow, foregroundWindowText, foregroundWindowText.Capacity);

                // Get the title of the console window
                string consoleWindowText = Console.Title;
                Console.WriteLine(consoleWindowText);
                Console.WriteLine(foregroundWindowText.ToString());

                // If the foreground window is the console window, handle the key press
                if (foregroundWindowText.ToString() == consoleWindowText)
                {
            */
                    // Do something with the key press here
                    switch((Keys)vkCode)
                    {
                        case Keys.MediaNextTrack:
                            Start.state = MainStates.next; // next song
                            break;
                        case Keys.MediaPreviousTrack:
                            Start.state = MainStates.previous; // previous song
                            break;
                        case Keys.MediaPlayPause:
                        case Keys.Pause:
                            Start.PauseSong();
                            Play.PlayDrawReset();
                            break;
                    }
                // }
            // }
    }

    return CallNextHookEx(_hookID, nCode, wParam, lParam);
}

    public void Dispose()
    {
        UnhookKeyboard();
    }
}
#endif