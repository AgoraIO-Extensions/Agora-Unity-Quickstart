using UnityEngine;
using System;
using System.Collections.Generic;

namespace AgoraNative
{
    [Serializable]
    public class MacWindowModel
    {
        // Sample model data
        //{
        //  "kCGWindowLayer": 0,
        //  "kCGWindowAlpha": 1,
        //  "kCGWindowMemoryUsage": 1152,
        //  "kCGWindowIsOnscreen": true,
        //  "kCGWindowSharingState": 0,
        //  "kCGWindowOwnerPID": 62356,
        //  "kCGWindowNumber": 30372,
        //  "kCGWindowOwnerName": "Unity",
        //  "kCGWindowStoreType": 1,
        //  "kCGWindowBounds": {
        //    "X": 1944,
        //    "Height": 961,
        //    "Y": 67,
        //    "Width": 1656
        //  },
        //  "kCGWindowName": "SceneHome - DesktopShare - PC, Mac & Linux Standalone - Unity 2019.3.2f1 Personal (Personal) <Metal>"
        //},

        public int kCGWindowLayer;
        public int kCGWindowAlpha;
        public int kCGWindowMemoryUsage;
        public bool kCGWindowIsOnscreen;
        public int kCGWindowSharingState;
        public int kCGWindowOwnerPID;
        public int kCGWindowNumber;
        public int kCGWindowStoreType;
        public string kCGWindowOwnerName;
        public string kCGWindowName;
        public WindowBounds kCGWindowBounds;
    }

    [Serializable]
    public class WindowBounds
    {
        public int X;
        public int Y;
        public int Height;
        public int Width;
    }

    [Serializable]
    public class WindowList
    {
        public List<MacWindowModel> windows;

        public static WindowList ParseJson(string listJson)
        {
            // Wrap the list in this class
            string json = ("{\"windows\":" + listJson + "}");
            try
            {
                WindowList macWindows = JsonUtility.FromJson<WindowList>(json);
                //Debug.LogWarning("JSON list count = " + macWindows.windows.Count);
                //foreach (MacWindowModel win in macWindows.windows)
                //{
                //    Debug.LogWarning(win.kCGWindowOwnerName + " => " + win.kCGWindowNumber);
                //}
                return macWindows;
            }
            catch (System.Exception e)
            {
                Debug.LogError("Exception in passing json, e:" + e);
                return null;
            }
        }
    }
}
