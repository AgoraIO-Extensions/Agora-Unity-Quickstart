using System.Collections.Generic;
using System.Collections.Concurrent;

using UnityEngine;
using UnityEngine.UI;
using agora_gaming_rtc;
using agora_utilities;

namespace CustomAudioSink
{
    /// <summary>
    ///    Sample code showing per user based audio frame data conversion to AudioSource
    ///    Requires .net v4.6 using BlockingCollection to implement the MainThreadDispatcher concept.
    /// </summary>
    public class UserAudioFrame2SourceSample : MonoBehaviour, IUserAudioFrameDelegate
    {
        public AudioRawDataManager.OnPlaybackAudioFrameBeforeMixingHandler HandleAudioFrameForUser { get; set; }
        [SerializeField] private string APP_ID = "YOUR_APPID";

        [SerializeField] private string TOKEN = "";

        [SerializeField] private string CHANNEL_NAME = "YOUR_CHANNEL_NAME";
        public Text logText;
#if NET_4_6
        private Logger logger;
        private IRtcEngine mRtcEngine = null;
        private IAudioRawDataManager _audioRawDataManager;

        private const float Offset = 100;

        Dictionary<uint, GameObject> RemoteUserObject = new Dictionary<uint, GameObject>();
        HashSet<uint> RemoteUserConfigured = new HashSet<uint>();

        BlockingCollection<System.Action> blockingCollection;

        private void Awake()
        {
            blockingCollection = new BlockingCollection<System.Action>();
        }

        // Start is called before the first frame update
        void Start()
        {
            bool appIdOK = CheckAppId();
            if (!appIdOK) return;

            InitRtcEngine();
            JoinChannel();
        }

        void Update()
        {
            PermissionHelper.RequestMicrophontPermission();
            System.Action action;
            while (blockingCollection.TryTake(out action)) action();
        }

        private void OnDestroy()
        {
            blockingCollection.Dispose();
        }

        public void dispatch(System.Action action)
        {
            blockingCollection.Add(action);
        }

        bool CheckAppId()
        {
            logger = new Logger(logText);
            return logger.DebugAssert(APP_ID.Length > 10, "Please fill in your appId in Canvas!!!!!");
        }

        void InitRtcEngine()
        {
            mRtcEngine = IRtcEngine.GetEngine(APP_ID);
            if (mRtcEngine == null)
            {
                logger.UpdateLog("Engine creation failure!!!! App not running");
                return;
            }

            mRtcEngine.SetLogFile("log.txt");
            mRtcEngine.OnJoinChannelSuccess += OnJoinChannelSuccessHandler;
            mRtcEngine.OnLeaveChannel += OnLeaveChannelHandler;
            mRtcEngine.OnWarning += OnSDKWarningHandler;
            mRtcEngine.OnError += OnSDKErrorHandler;
            mRtcEngine.OnConnectionLost += OnConnectionLostHandler;
            mRtcEngine.OnUserJoined += OnUserJoinedHandler;
            mRtcEngine.OnUserOffline += OnUserOfflineHandler;

            _audioRawDataManager = AudioRawDataManager.GetInstance(mRtcEngine);
            Debug.Assert(_audioRawDataManager.RegisterAudioRawDataObserver() == 0, "Error registering audio rawdata observer!");
            mRtcEngine.SetParameter("che.audio.external_render", true);
        }

        void JoinChannel()
        {
            mRtcEngine.EnableVideo();
            mRtcEngine.EnableVideoObserver();
            mRtcEngine.JoinChannelByKey(TOKEN, CHANNEL_NAME, "", 0);
        }

        void OnApplicationQuit()
        {
            Debug.Log("OnApplicationQuit");
            if (mRtcEngine != null)
            {
                IRtcEngine.Destroy();
            }
        }

        void OnJoinChannelSuccessHandler(string channelName, uint uid, int elapsed)
        {
            logger.UpdateLog(string.Format("sdk version: {0}", IRtcEngine.GetSdkVersion()));
            logger.UpdateLog(string.Format("onJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}", channelName,
                uid, elapsed));

            _audioRawDataManager.SetOnPlaybackAudioFrameBeforeMixingCallback(OnPlaybackAudioFrameBeforeMixingHandler);
        }

        void OnUserOfflineHandler(uint uid, USER_OFFLINE_REASON reason)
        {
            Debug.LogWarning("User " + uid + " went offline, reason:" + reason);
            lock (RemoteUserConfigured)
            {
                if (RemoteUserConfigured.Contains(uid))
                {
                    RemoteUserConfigured.Remove(uid);
                    Destroy(RemoteUserObject[uid]);
                    RemoteUserObject.Remove(uid);
                }
            }
        }

        void OnLeaveChannelHandler(RtcStats stats)
        {
            logger.UpdateLog("OnLeaveChannelSuccess");
        }

        void OnSDKWarningHandler(int warn, string msg)
        {
            logger.UpdateLog(string.Format("OnSDKWarning warn: {0}, msg: {1}", warn, msg));
        }

        void OnSDKErrorHandler(int error, string msg)
        {
            logger.UpdateLog(string.Format("OnSDKError error: {0}, msg: {1}", error, msg));
        }

        void OnConnectionLostHandler()
        {
            logger.UpdateLog(string.Format("OnConnectionLost "));
        }

        void OnPlaybackAudioFrameBeforeMixingHandler(uint uid, AudioFrame audioFrame)
        {

            // The audio stream info contains in this audioframe, we will use this construct the AudioClip
            lock (RemoteUserConfigured)
            {
                if (!RemoteUserConfigured.Contains(uid) && RemoteUserObject.ContainsKey(uid))
                {
                    GameObject go = RemoteUserObject[uid];
                    if (go != null)
                    {
                        dispatch(() =>
                       {
                           UserAudioFrameHandler userAudio = go.AddComponent<UserAudioFrameHandler>();
                           userAudio.Init(uid, this, audioFrame);
                           RemoteUserConfigured.Add(uid);
                       });
                    }
                }
            }

            if (HandleAudioFrameForUser != null)
            {
                HandleAudioFrameForUser(uid, audioFrame);
            }
        }

        void OnUserJoinedHandler(uint uid, int elapsed)
        {
            logger.UpdateLog(string.Format("OnUserJoined uid: ${0} elapsed: ${1}", uid, elapsed));
            GameObject go = makeVideoView(uid);
            RemoteUserObject[uid] = go;
        }

        private GameObject makeVideoView(uint uid)
        {
            GameObject go = GameObject.Find(uid.ToString());
            if (!ReferenceEquals(go, null))
            {
                return go; // reuse
            }

            // create a GameObject and assign to this new user
            VideoSurface videoSurface = makeImageSurface(uid.ToString());
            if (!ReferenceEquals(videoSurface, null))
            {
                // configure videoSurface
                videoSurface.SetForUser(uid);
                videoSurface.SetEnable(true);
                videoSurface.SetVideoSurfaceType(AgoraVideoSurfaceType.RawImage);
                videoSurface.SetGameFps(30);
            }

            return videoSurface.gameObject;
        }

        public VideoSurface makeImageSurface(string goName)
        {
            GameObject go = new GameObject();

            if (go == null)
            {
                return null;
            }

            go.name = goName;
            // to be renderered onto
            go.AddComponent<RawImage>();
            // make the object draggable
            go.AddComponent<UIElementDrag>();
            GameObject canvas = GameObject.Find("Canvas");
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
            go.transform.Rotate(0f, 0.0f, 180.0f);
            float xPos = UnityEngine.Random.Range(Offset - Screen.width / 2f, Screen.width / 2f - Offset);
            float yPos = UnityEngine.Random.Range(Offset, Screen.height / 2f - Offset);
            Debug.Log("position x " + xPos + " y: " + yPos);
            go.transform.localPosition = new Vector3(xPos, yPos, 0f);
            go.transform.localScale = new Vector3(1f, 1.6f, 1f);

            // configure videoSurface
            VideoSurface videoSurface = go.AddComponent<VideoSurface>();
            return videoSurface;
        }
#else
        public string USE_NET46 = "PLEASE USE .NET 4.6!";
        void Start()
        {
            Debug.LogError("PLease use .Net 4.6 to run this demo!!");
	    }
#endif
    }
}