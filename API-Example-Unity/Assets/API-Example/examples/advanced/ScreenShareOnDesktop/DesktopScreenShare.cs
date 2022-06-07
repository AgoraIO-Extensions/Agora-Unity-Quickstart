using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using agora_gaming_rtc;
using agora_utilities;
using AgoraNative;
using Random = UnityEngine.Random;

public class DesktopScreenShare : MonoBehaviour
{
    [SerializeField]
    public string APP_ID = "";

    [SerializeField]
    public string TOKEN = "";

    [SerializeField]
    public string CHANNEL_NAME = "YOUR_CHANNEL_NAME";

    private IRtcEngine _rtcEngine;
    private uint _remoteUid = 0;
    private const float _offset = 100;
    public Text LogText;
    private Logger _logger;
    private Dropdown _winIdSelect;
    private Button _startShareBtn;
    private Button _stopShareBtn;

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
    private Dictionary<uint, AgoraNativeBridge.RECT> _dispRect;
#endif

    // Use this for initialization
    void Start()
    {
        _logger = new Logger(LogText);
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        _dispRect = new Dictionary<uint, AgoraNativeBridge.RECT>();
#endif
        CheckAppId();
        InitEngine();
        JoinChannel();
        PrepareScreenCapture();
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void CheckAppId()
    {
        _logger.DebugAssert(APP_ID.Length > 10, "Please fill in your appId in VideoCanvas!!!!!");
    }

    private void JoinChannel()
    {
        _rtcEngine.JoinChannelByKey(TOKEN, CHANNEL_NAME);
    }

    private void InitEngine()
    {
        _rtcEngine = IRtcEngine.GetEngine(APP_ID);
        _rtcEngine.SetLogFile("log.txt");
        _rtcEngine.EnableAudio();
        _rtcEngine.EnableVideo();
        _rtcEngine.EnableVideoObserver();
        _rtcEngine.OnJoinChannelSuccess += OnJoinChannelSuccessHandler;
        _rtcEngine.OnLeaveChannel += OnLeaveChannelHandler;
        _rtcEngine.OnWarning += OnSDKWarningHandler;
        _rtcEngine.OnError += OnSDKErrorHandler;
        _rtcEngine.OnConnectionLost += OnConnectionLostHandler;
        _rtcEngine.OnUserJoined += OnUserJoinedHandler;
        _rtcEngine.OnUserOffline += OnUserOfflineHandler;
    }


    private void PrepareScreenCapture()
    {
        _winIdSelect = GameObject.Find("winIdSelect").GetComponent<Dropdown>();

        if (_winIdSelect != null)
        {
            _winIdSelect.ClearOptions();
#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
            var macDispIdList = AgoraNativeBridge.GetMacDisplayIds();
            if (macDispIdList != null)
            {
                _winIdSelect.AddOptions(macDispIdList.Select(w =>
                    new Dropdown.OptionData(
                        string.Format("Display {0}", w))).ToList());
            }

            var macWinIdList = AgoraNativeBridge.GetMacWindowList();
            if (macWinIdList != null)
            {
                _winIdSelect.AddOptions(macWinIdList.windows.Select(w =>
                        new Dropdown.OptionData(
                            string.Format("{0, -20} | {1}", w.kCGWindowOwnerName, w.kCGWindowNumber)))
                    .ToList());
            }
#elif UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            var winDispInfoList = AgoraNativeBridge.GetWinDisplayInfo();
            if (winDispInfoList != null)
            {
                foreach (var dpInfo in winDispInfoList)
                {
                    _dispRect.Add(dpInfo.MonitorInfo.flags, dpInfo.MonitorInfo.monitor);
                }

                _winIdSelect.AddOptions(winDispInfoList.Select(w =>
                    new Dropdown.OptionData(
                        string.Format("Display {0}", w.MonitorInfo.flags))).ToList());
            }

            Dictionary<string, IntPtr> winWinIdList;
            AgoraNativeBridge.GetDesktopWindowHandlesAndTitles(out winWinIdList);
            if (winWinIdList != null)
            {
                _winIdSelect.AddOptions(winWinIdList.Select(w =>
                    new Dropdown.OptionData(string.Format("{0, -20} | {1}",
                        w.Key.Substring(0, Math.Min(w.Key.Length, 20)), w.Value))).ToList());
            }
#endif
        }

        _startShareBtn = GameObject.Find("startShareBtn").GetComponent<Button>();
        _stopShareBtn = GameObject.Find("stopShareBtn").GetComponent<Button>();
        if (_startShareBtn != null) _startShareBtn.onClick.AddListener(OnStartShareBtnClick);
        if (_stopShareBtn != null)
        {
            _stopShareBtn.onClick.AddListener(OnStopShareBtnClick);
            _stopShareBtn.gameObject.SetActive(false);
        }
    }

    private void OnStartShareBtnClick()
    {
        if (_startShareBtn != null) _startShareBtn.gameObject.SetActive(false);
        if (_stopShareBtn != null) _stopShareBtn.gameObject.SetActive(true);
        _rtcEngine.StopScreenCapture();

        if (_winIdSelect == null) return;
        var option = _winIdSelect.options[_winIdSelect.value].text;
        if (string.IsNullOrEmpty(option)) return;
        if (option.Contains("|"))
        {
            var windowId = option.Split("|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[1];
            _logger.UpdateLog(string.Format(">>>>> Start sharing {0}", windowId));
            _rtcEngine.StartScreenCaptureByWindowId(int.Parse(windowId), default(Rectangle),
                default(ScreenCaptureParameters));
        }
        else
        {
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            var dispId = uint.Parse(option.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[1]);
            _logger.UpdateLog(string.Format(">>>>> Start sharing display {0}", dispId));
            _rtcEngine.StartScreenCaptureByDisplayId(dispId, default(Rectangle),
                new ScreenCaptureParameters {captureMouseCursor = true, frameRate = 30});
#elif UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            var diapFlag = uint.Parse(option.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[1]);
            var screenRect = new Rectangle
            {
                x = _dispRect[diapFlag].left,
                y = _dispRect[diapFlag].top,
                width = _dispRect[diapFlag].right - _dispRect[diapFlag].left,
                height = _dispRect[diapFlag].bottom - _dispRect[diapFlag].top
            };
            _logger.UpdateLog(string.Format(">>>>> Start sharing display {0}: {1} {2} {3} {4}", diapFlag, screenRect.x,
                screenRect.y, screenRect.width, screenRect.height));
            var ret = _rtcEngine.StartScreenCaptureByScreenRect(screenRect,
                new Rectangle {x = 0, y = 0, width = 0, height = 0}, default(ScreenCaptureParameters));
#endif
        }
    }

    private void OnStopShareBtnClick()
    {
        if (_startShareBtn != null) _startShareBtn.gameObject.SetActive(true);
        if (_stopShareBtn != null) _stopShareBtn.gameObject.SetActive(false);
        _rtcEngine.StopScreenCapture();
    }

    private void OnJoinChannelSuccessHandler(string channelName, uint uid, int elapsed)
    {
        _logger.UpdateLog(string.Format("sdk version: ${0}", IRtcEngine.GetSdkVersion()));
        _logger.UpdateLog(string.Format("onJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}", channelName,
            uid, elapsed));
        makeVideoView(0);
    }

    private void OnLeaveChannelHandler(RtcStats stats)
    {
        _logger.UpdateLog("OnLeaveChannelSuccess");
        DestroyVideoView(0);
    }

    private void OnUserJoinedHandler(uint uid, int elapsed)
    {
        if (_remoteUid == 0) _remoteUid = uid;
        _logger.UpdateLog(string.Format("OnUserJoined uid: ${0} elapsed: ${1}", uid, elapsed));
        makeVideoView(uid);
    }

    private void OnUserOfflineHandler(uint uid, USER_OFFLINE_REASON reason)
    {
        _remoteUid = 0;
        _logger.UpdateLog(string.Format("OnUserOffLine uid: ${0}, reason: ${1}", uid, (int) reason));
        DestroyVideoView(uid);
    }

    private void OnSDKWarningHandler(int warn, string msg)
    {
        _logger.UpdateLog(string.Format("OnSDKWarning warn: {0}, msg: {1}", warn, msg));
    }

    private void OnSDKErrorHandler(int error, string msg)
    {
        _logger.UpdateLog(string.Format("OnSDKError error: {0}, msg: {1}", error, msg));
    }

    private void OnConnectionLostHandler()
    {
        _logger.UpdateLog("OnConnectionLost ");
    }

    void OnApplicationQuit()
    {
        Debug.Log("OnApplicationQuit");
        if (_rtcEngine != null)
        {
            _rtcEngine.LeaveChannel();
            _rtcEngine.DisableVideoObserver();
            IRtcEngine.Destroy();
        }
    }

    private void DestroyVideoView(uint uid)
    {
        var go = GameObject.Find(uid.ToString());
        if (!ReferenceEquals(go, null))
        {
            Destroy(go);
        }
    }

    private void makeVideoView(uint uid)
    {
        var go = GameObject.Find(uid.ToString());
        if (!ReferenceEquals(go, null))
        {
            return; // reuse
        }

        // create a GameObject and assign to this new user
        var videoSurface = makeImageSurface(uid.ToString());
        if (!ReferenceEquals(videoSurface, null))
        {
            // configure videoSurface
            videoSurface.SetForUser(uid);
            videoSurface.SetEnable(true);
            videoSurface.SetVideoSurfaceType(AgoraVideoSurfaceType.RawImage);
            videoSurface.SetGameFps(30);
            videoSurface.EnableFilpTextureApply(true, false);
        }
    }

    // VIDEO TYPE 1: 3D Object
    public VideoSurface makePlaneSurface(string goName)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Plane);

        if (go == null)
        {
            return null;
        }

        go.name = goName;
        // set up transform
        go.transform.Rotate(-90.0f, 0.0f, 0.0f);
        var yPos = Random.Range(3.0f, 5.0f);
        var xPos = Random.Range(-2.0f, 2.0f);
        go.transform.position = new Vector3(xPos, yPos, 0f);
        go.transform.localScale = new Vector3(0.25f, 0.5f, .5f);

        // configure videoSurface
        var videoSurface = go.AddComponent<VideoSurface>();
        return videoSurface;
    }

    // Video TYPE 2: RawImage
    public VideoSurface makeImageSurface(string goName)
    {
        var go = new GameObject();

        if (go == null)
        {
            return null;
        }

        go.name = goName;
        // to be renderered onto
        go.AddComponent<RawImage>();
        // make the object draggable
        go.AddComponent<UIElementDrag>();
        var canvas = GameObject.Find("VideoCanvas");
        if (canvas != null)
        {
            go.transform.parent = canvas.transform;
            Debug.Log("add video view");
        }
        else
        {
            Debug.Log("Canvas is null video view");
        }

        // set up transform
        go.transform.Rotate(0.0f, 0.0f, 180.0f);
        var xPos = Random.Range(_offset - Screen.width / 2f, Screen.width / 2f - _offset);
        var yPos = Random.Range(_offset, Screen.height / 2f - _offset);
        go.transform.localPosition = new Vector3(xPos, yPos, 0f);
        go.transform.localScale = new Vector3(3f, 4f, 1f);

        // configure videoSurface
        var videoSurface = go.AddComponent<VideoSurface>();
        return videoSurface;
    }
}