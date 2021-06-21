using System.Collections.Generic;
using System.Collections.Concurrent;
using UnityEngine;
using UnityEngine.UI;
using agora_gaming_rtc;

namespace agora_sample_code
{
    /// <summary>
    ///   Demo for sending individual audio stream into audio source instance.
    ///   This demo does not manage local user's camera for simplicity.
    /// </summary>
    public class UserAudioFrame2SourceSample : MonoBehaviour, IUserAudioFrameDelegate
    {
        [SerializeField]
        private string APP_ID = "";

        [SerializeField]
        private string TOKEN = "";

        [SerializeField]
        private string CHANNEL_NAME = "YOUR_CHANNEL_NAME";
        public Text logText;
        private Logger logger;
        private IRtcEngine mRtcEngine = null;

        [SerializeField] Transform rootSpace;
        [SerializeField] GameObject userPrefab;

        private IAudioRawDataManager _audioRawDataManager;
        public AudioRawDataManager.OnPlaybackAudioFrameBeforeMixingHandler HandleAudioFrameForUser
        {
            get; set;
        }

#if NET_4_6 || NET_STANDARD_2_0
        BlockingCollection<System.Action> blockingCollection;
        Dictionary<uint, GameObject> RemoteUserObject = new Dictionary<uint, GameObject>();
        HashSet<uint> RemoteUserConfigured = new HashSet<uint>();

        private void Awake()
        {
            blockingCollection = new BlockingCollection<System.Action>();
            if (userPrefab == null)
            {
                Debug.LogWarning("User prefab wasn't assigned, generating primitive object as prefab.");
                MakePrefab();
            }
        }

        void Start()
        {
            PermissionHelper.RequestMicrophontPermission();
            PermissionHelper.RequestCameraPermission();
            CheckAppId();
            InitEngine();
            JoinChannel();
        }

        void Update()
        {
            System.Action action;
            while (blockingCollection.TryTake(out action)) action();
        }

        void CheckAppId()
        {
            logger = new Logger(logText);
            logger.DebugAssert(APP_ID.Length > 10, "Please fill in your appId in VideoCanvas!!!!!");
        }

        void InitEngine()
        {
            mRtcEngine = IRtcEngine.GetEngine(APP_ID);
            mRtcEngine.SetLogFile("log.txt");
            //mRtcEngine.SetChannelProfile(CHANNEL_PROFILE.CHANNEL_PROFILE_LIVE_BROADCASTING);
            //mRtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
            mRtcEngine.EnableAudio();
            mRtcEngine.EnableVideo();
            mRtcEngine.EnableLocalVideo(false);
            mRtcEngine.EnableVideoObserver();
            mRtcEngine.OnJoinChannelSuccess += OnJoinChannelSuccessHandler;
            mRtcEngine.OnLeaveChannel += OnLeaveChannelHandler;
            mRtcEngine.OnWarning += OnSDKWarningHandler;
            mRtcEngine.OnError += OnSDKErrorHandler;
            mRtcEngine.OnConnectionLost += OnConnectionLostHandler;
            mRtcEngine.OnUserJoined += OnUserJoinedHandler;
            mRtcEngine.OnUserOffline += OnUserOfflineHandler;
        }

        void JoinChannel()
        {
            _audioRawDataManager = AudioRawDataManager.GetInstance(mRtcEngine);
            _audioRawDataManager.RegisterAudioRawDataObserver();

            mRtcEngine.SetParameter("che.audio.external_render", true);
            mRtcEngine.JoinChannelByKey(TOKEN, CHANNEL_NAME, "", 0);
        }
        void OnJoinChannelSuccessHandler(string channelName, uint uid, int elapsed)
        {
            logger.UpdateLog(string.Format("sdk version: ${0}", IRtcEngine.GetSdkVersion()));
            logger.UpdateLog(string.Format("onJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}", channelName, uid, elapsed));
            // makeVideoView(0);

            _audioRawDataManager.SetOnPlaybackAudioFrameBeforeMixingCallback(OnPlaybackAudioFrameBeforeMixingHandler);

        }

        void OnLeaveChannelHandler(RtcStats stats)
        {
            logger.UpdateLog("OnLeaveChannelSuccess");
        }

        int userCount = 0;
        void OnUserJoinedHandler(uint uid, int elapsed)
        {
            logger.UpdateLog(string.Format("OnUserJoined uid: ${0} elapsed: ${1}", uid, elapsed));
            GameObject go = Instantiate(userPrefab);
            RemoteUserObject[uid] = go;
            go.transform.SetParent(rootSpace);
            go.transform.localScale = Vector3.one;
            go.transform.localPosition = new Vector3(userCount * 2, 0, 0);

            VideoSurface v = go.AddComponent<VideoSurface>();
            v.SetForUser(uid);
            v.SetEnable(true);
            v.SetVideoSurfaceType(AgoraVideoSurfaceType.Renderer);
            userCount++;
        }

        void OnUserOfflineHandler(uint uid, USER_OFFLINE_REASON reason)
        {
            logger.UpdateLog("User " + uid + " went offline, reason:" + reason);
            dispatch(() => { logger.UpdateLog("Dispatched log + OFFLINE reason = " + reason); });

            lock (RemoteUserConfigured)
            {
                if (RemoteUserObject.ContainsKey(uid))
                {
                    Destroy(RemoteUserObject[uid]);
                    RemoteUserObject.Remove(uid);
                }

                if (RemoteUserConfigured.Contains(uid))
                {
                    RemoteUserConfigured.Remove(uid);
                }
            }
        }

        void OnSDKWarningHandler(int warn, string msg)
        {
            logger.UpdateLog(string.Format("OnSDKWarning warn: {0}, msg: {1}", warn, IRtcEngine.GetErrorDescription(warn)));
        }

        void OnSDKErrorHandler(int error, string msg)
        {
            logger.UpdateLog(string.Format("OnSDKError error: {0}, msg: {1}", error, IRtcEngine.GetErrorDescription(error)));
        }

        void OnConnectionLostHandler()
        {
            logger.UpdateLog(string.Format("OnConnectionLost "));
        }

        public void dispatch(System.Action action)
        {
            blockingCollection.Add(action);
        }

        int count = 0;
        const int MAXAUC = 5;
        void OnPlaybackAudioFrameBeforeMixingHandler(uint uid, AudioFrame audioFrame)
        {
            // limited log
            if (count < MAXAUC)
                Debug.LogWarning("count(" + count + "): OnPlaybackAudioFrameBeforeMixingHandler =============+> " + audioFrame);
            count++;

            // The audio stream info contains in this audioframe, we will use this construct the AudioClip
            lock (RemoteUserConfigured)
            {
                if (!RemoteUserConfigured.Contains(uid) && RemoteUserObject.ContainsKey(uid))
                {
                    if (count < MAXAUC)
                        dispatch(() =>
                        {
                            logger.UpdateLog("Uid:" + uid + " setting up audio frame handler....");
                        });

                    GameObject go = RemoteUserObject[uid];
                    if (go != null)
                    {
                        dispatch(() =>
                        {
                            UserAudioFrameHandler userAudio = go.GetComponent<UserAudioFrameHandler>();
                            if (userAudio == null)
                            {
                                userAudio = go.AddComponent<UserAudioFrameHandler>();
                                userAudio.Init(uid, this, audioFrame);
                                RemoteUserConfigured.Add(uid);
                            }
                            go.SetActive(true);
                        });
                    }
                    else
                    {
                        dispatch(() =>
                        {
                            logger.UpdateLog("Uid: " + uid + " setting up audio frame handler._<> no go");
                        });
                    }
                }
            }

            if (HandleAudioFrameForUser != null)
            {
                HandleAudioFrameForUser(uid, audioFrame);
            }
        }

        void OnApplicationQuit()
        {
            Debug.Log("OnApplicationQuit");
            if (mRtcEngine != null)
            {
                mRtcEngine.LeaveChannel();
                mRtcEngine.DisableVideoObserver();
                if (_audioRawDataManager != null)
                {
                    AudioRawDataManager.ReleaseInstance();
                }
                IRtcEngine.Destroy();
            }
        }

        protected virtual void MakePrefab()
        {
            Debug.LogWarning("Generating cube as prefab.");
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            userPrefab = go;
            go.transform.SetPositionAndRotation(Vector3.zero, Quaternion.Euler(0, 45f, 45f));
            MeshRenderer mesh = GetComponent<MeshRenderer>();
            if (mesh != null)
            {
                mesh.material = new Material(Shader.Find("Unlit/Texture"));
            }
            go.SetActive(false);
        }
#else
    public string USE_NET46 = "PLEASE USE .NET 4.6 or Standard 2.0";
    void Start()
    {
        Debug.LogError("PLease use .Net 4.6 or standard 2.0 to run this demo!!");
    }
#endif

    }
}
