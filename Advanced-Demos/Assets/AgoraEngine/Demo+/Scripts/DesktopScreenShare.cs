using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

using agora_gaming_rtc;
using AgoraNative;

// this is an example of using Agora Unity SDK
// It demonstrates:
// How to enable video
// How to join/leave channel
// 
public class DesktopScreenShare : PlayerViewControllerBase
{

    Dropdown WindowOptionDropdown;

    protected override void SetupUI()
    {
        base.SetupUI();

        Dropdown dropdown = GameObject.Find("Dropdown").GetComponent<Dropdown>();
        if (dropdown != null)
        {
#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
            WindowList list = AgoraNativeBridge.GetMacWindowList();
            if (list != null)
            {
                dropdown.options = list.windows.Select(w =>
                    new Dropdown.OptionData(w.kCGWindowOwnerName + "|" + w.kCGWindowNumber)).ToList();
            }
            GameObject.Find("InputField").SetActive(false);
            GameObject.Find("WinHelpButton").SetActive(false);
#else
            dropdown.gameObject.SetActive(false);
            inputField = GameObject.Find("InputField").GetComponent<InputField>();
#endif
            WindowOptionDropdown = dropdown;
        }

        Button button = GameObject.Find("ShareWindowButton").GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnShareWindowClick);
        }

        button = GameObject.Find("ShareDisplayButton").GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(ShareDisplayScreen);
        }

        button = GameObject.Find("StopShareButton").GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(() => { mRtcEngine.StopScreenCapture(); });
        }

        GameObject quad = GameObject.Find("DisplayPlane");
        if (ReferenceEquals(quad, null))
        {
            Debug.Log("Error: failed to find DisplayPlane");
            return;
        }
        else
        {
            quad.AddComponent<VideoSurface>();
        }
    }

    int displayID0or1 = 0;
    void ShareDisplayScreen()
    {
        mRtcEngine.SetEncryptionSecret("HelloWorld1121414144");
        ScreenCaptureParameters sparams = new ScreenCaptureParameters
        {
            captureMouseCursor = true,
            frameRate = 30
        };

        mRtcEngine.StopScreenCapture();

#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        mRtcEngine.StartScreenCaptureByDisplayId(getDisplayId(displayID0or1), default(Rectangle), sparams);  // 
#else
        TestRectCrop(displayID0or1);
#endif
        displayID0or1 = 1 - displayID0or1;
    }

    void TestRectCrop(int order)
    {
        // Assuming you have two display monitors, each of 1920x1080, position left to right:
        Rectangle screenRect = new Rectangle() { x = 0, y = 0, width = 1920 * 2, height = 1080 };
        Rectangle regionRect = new Rectangle() { x = order * 1920, y = 0, width = 1920, height = 1080 };

        int rc = mRtcEngine.StartScreenCaptureByScreenRect(screenRect,
            regionRect,
            default(ScreenCaptureParameters)
            );
        if (rc != 0) Debug.LogWarning("rc = " + rc);
    }

    uint getDisplayId(int k)
    {
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        List<uint> ids = AgoraNativeBridge.GetMacDisplayIds();
#else
        List<uint> ids = new List<uint>();
#endif

        if (k < ids.Count)
        {
            return ids[k];
        }
        return 0;
    }

    private InputField inputField;

    void OnShareWindowClick()
    {
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX

        char[] delimiterChars = { '|' };
        if (WindowOptionDropdown == null) return;
        string option = WindowOptionDropdown.options[WindowOptionDropdown.value].text;
        if (string.IsNullOrEmpty(option))
        {
            return;
        }

        string wid = option.Split(delimiterChars, System.StringSplitOptions.RemoveEmptyEntries)[1];
        Debug.LogWarning(wid + " is chosen");
        mRtcEngine.StopScreenCapture();

        mRtcEngine.StartScreenCaptureByWindowId(int.Parse(wid), default(Rectangle), default(ScreenCaptureParameters));

#elif UNITY_EDITOR_WIN ||  UNITY_STANDALONE_WIN

        int winHandle;
        if (int.TryParse(inputField.text, out winHandle))
        {
            mRtcEngine.StopScreenCapture();
            mRtcEngine.StartScreenCaptureByWindowId(winHandle, default(Rectangle), default(ScreenCaptureParameters));
        }

#endif

    }
}
