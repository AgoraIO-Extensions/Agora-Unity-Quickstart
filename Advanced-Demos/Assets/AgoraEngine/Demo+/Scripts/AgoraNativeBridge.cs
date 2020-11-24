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
