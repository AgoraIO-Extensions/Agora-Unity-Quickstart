using System;
using System.Text;
using UnityEngine;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AgoraNative;

public static class AgoraNativeBridge
{
    const int DisplayCountMax = 10;
    const int CharBufferSize = 100000;

    // #if UNITY_IPHONE
    // On iOS plugins are statically linked into
    // the executable, so we have to use __Internal as the
    // library name.
    //     [DllImport ("__Internal")]

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
    // Save window titles and handles in these lists.
    private static Dictionary<string, IntPtr> windowInfo;
    private static List<MonitorInfoWithHandle> displayInfo;

    public static MonitorInfoWithHandle[] GetWinDisplayInfo()
    {
        displayInfo = new List<MonitorInfoWithHandle>();
        EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, FilterDisplayCallback, IntPtr.Zero);
        return displayInfo.ToArray();
    }

    // Return a list of the desktop windows' handles and titles.
    public static void GetDesktopWindowHandlesAndTitles(
        out Dictionary<string, IntPtr> info)
    {
        windowInfo = new Dictionary<string, IntPtr>();

        if (!EnumDesktopWindows(IntPtr.Zero, FilterCallback,
            IntPtr.Zero))
        {
            info = null;
        }
        else
        {
            info = windowInfo;
        }
    }

    // We use this function to filter windows.
    // This version selects visible windows that have titles.
    private static bool FilterCallback(IntPtr hWnd, int lParam)
    {
        // Get the window's title.
        StringBuilder sb_title = new StringBuilder(1024);
        GetWindowText(hWnd, sb_title, sb_title.Capacity);
        string title = sb_title.ToString();

        // If the window is visible and has a title, save it.
        if (IsWindowVisible(hWnd) &&
            string.IsNullOrEmpty(title) == false)
        {
            if (windowInfo.ContainsKey(title)) title = string.Format("{0}{1}", title, hWnd);
            windowInfo.Add(title, hWnd);
        }

        // Return true to indicate that we
        // should continue enumerating windows.
        return true;
    }

    private static bool FilterDisplayCallback(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData)
    {
        var mi = new MONITORINFO();
        mi.size = (uint) Marshal.SizeOf(mi);
        GetMonitorInfo(hMonitor, ref mi);

        // Add to monitor info
        displayInfo.Add(new MonitorInfoWithHandle(hMonitor, mi));
        return true;
    }

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll", EntryPoint = "GetWindowText",
        ExactSpelling = false, CharSet = CharSet.Auto, SetLastError = true)]
    private static extern int GetWindowText(IntPtr hWnd,
        StringBuilder lpWindowText, int nMaxCount);

    [DllImport("user32.dll", EntryPoint = "EnumDesktopWindows",
        ExactSpelling = false, CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool EnumDesktopWindows(IntPtr hDesktop,
        EnumDelegate lpEnumCallbackFunction, IntPtr lParam);

    [DllImport("user32.dll", EntryPoint = "EnumDisplayMonitors",
        ExactSpelling = false, CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip,
        EnumMonitorsDelegate lpEnumCallbackFunction, IntPtr dwData);

    [DllImport("user32.dll", EntryPoint = "GetMonitorInfo",
        ExactSpelling = false, CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool GetMonitorInfo(IntPtr hmon, ref MONITORINFO mi);

    // Define the callback delegate's type.
    private delegate bool EnumDelegate(IntPtr hWnd, int lParam);

    private delegate bool EnumMonitorsDelegate(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData);

    // Monitor information with handle interface.
    public interface IMonitorInfoWithHandle
    {
        IntPtr MonitorHandle { get; }
        MONITORINFO MonitorInfo { get; }
    }

    public class MonitorInfoWithHandle : IMonitorInfoWithHandle
    {
        public IntPtr MonitorHandle { get; private set; }
        public MONITORINFO MonitorInfo { get; private set; }

        public MonitorInfoWithHandle(IntPtr monitorHandle, MONITORINFO monitorInfo)
        {
            MonitorHandle = monitorHandle;
            MonitorInfo = monitorInfo;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MONITORINFO
    {
        public uint size;
        public RECT monitor;
        public RECT work;
        public uint flags;
    }
#endif

#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
    // Other platforms load plugins dynamically, so pass the name
    // of the plugin's dynamic library.
    [DllImport("ShareScreenLib")]
    private static extern int PrintANumber();

    [DllImport("ShareScreenLib")]
    private static extern void GetDisplayIds([In, Out] uint[] displays, ref int size, int length);

    [DllImport("ShareScreenLib")]
    private static extern void GetWindows(StringBuilder stringBuilder, int capacity);

    public static void TestMacBridge()
    {
        int k = PrintANumber();
        Debug.LogWarning("ver number = " + k);

        List<uint> ids = GetMacDisplayIds();

        for (int i = 0; i < ids.Count; i++)
        {
            Debug.LogWarning("Id = " + ids[i]);
        }

        GetMacWindowList();
    }

    public static List<uint> GetMacDisplayIds()
    {
        uint[] ids = new uint[DisplayCountMax];
        int size = 0;  // how much of array filled
        List<uint> list = new List<uint>();

        GetDisplayIds(ids, ref size, ids.Length);
        for (int i = 0; i < size; i++)
        {
            list.Add(ids[i]);
        }
        return list;
    }

    public static WindowList GetMacWindowList()
    {
        StringBuilder sb = new StringBuilder(CharBufferSize);
        GetWindows(sb, sb.Capacity);

        WindowList macWindows = WindowList.ParseJson(sb.ToString());
        if (macWindows != null)
        {
            Debug.LogWarning("JSON list count = " + macWindows.windows.Count);
            foreach (MacWindowModel win in macWindows.windows)
            {
                Debug.LogWarning(win.kCGWindowOwnerName + " => " + win.kCGWindowNumber);
            }
        }
        return macWindows;
    }
#endif
}