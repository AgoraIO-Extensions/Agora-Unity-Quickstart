using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using agora_gaming_rtc;
using Threading;

namespace CustomAudioSink
{
    public class CustomAudioSinkSample : MonoBehaviour
    {
        [SerializeField] private string APP_ID = "YOUR_APPID";

        [SerializeField] private string TOKEN = "";

        [SerializeField] private string CHANNEL_NAME = "YOUR_CHANNEL_NAME";
        public Text logText;
        private Logger logger;
        private IRtcEngine mRtcEngine = null;
        private IAudioRawDataManager _audioRawDataManager;
        private int CHANNEL = 1;
        private int SAMPLE_RATE = 44100;
        private bool externalRenderAudio = false;

        private readonly Queue<Action> _actionQueue = new Queue<Action>();

        public Queue<Action> ActionQueue
        {
            get
            {
                lock (Async.GetLock("ActionQueue"))
                {
                    return _actionQueue;
                }
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            CheckAppId();
            InitRtcEngine();
            JoinChannel();
            MultiMultiMulti();
        }

        void Update()
        {
            PermissionHelper.RequestMicrophontPermission();
        }

        void CheckAppId()
        {
            logger = new Logger(logText);
            logger.DebugAssert(APP_ID.Length > 10, "Please fill in your appId in Canvas!!!!!");
        }

        void InitRtcEngine()
        {
            mRtcEngine = IRtcEngine.GetEngine(APP_ID);
            mRtcEngine.SetExternalAudioSink(true, SAMPLE_RATE, CHANNEL);
            mRtcEngine.SetLogFile("log.txt");
            mRtcEngine.OnJoinChannelSuccess += OnJoinChannelSuccessHandler;
            mRtcEngine.OnLeaveChannel += OnLeaveChannelHandler;
            mRtcEngine.OnWarning += OnSDKWarningHandler;
            mRtcEngine.OnError += OnSDKErrorHandler;
            mRtcEngine.OnConnectionLost += OnConnectionLostHandler;
            _audioRawDataManager = AudioRawDataManager.GetInstance(mRtcEngine);
            externalRenderAudio = true;
        }

        // private void Awake()
        // {
        //     var childRef = new ThreadStart(PullAudioFrameThread);
        //     var childThread = new Thread(childRef);
        //     childThread.Start();
        // }

        void JoinChannel()
        {
            mRtcEngine.JoinChannelByKey(TOKEN, CHANNEL_NAME, "", 0);
        }

        void OnLeaveBtnClick()
        {
            mRtcEngine.LeaveChannel();
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

        private void MultiMultiMulti()
        {
            Async.RunInBackground("MultiMultiMulti", 10, () =>
            {
                var avsync_type = 0;
                var bytesPerSample = 2;
                var type = AUDIO_FRAME_TYPE.FRAME_TYPE_PCM16;
                var channels = CHANNEL;
                var samples = SAMPLE_RATE / 100 * CHANNEL;
                var samplesPerSec = SAMPLE_RATE;
                var buffer = Marshal.AllocHGlobal(samples * bytesPerSample);

                _audioRawDataManager.PullAudioFrame(buffer, (int) type, samples, bytesPerSample, channels,
                    samplesPerSec, 0, avsync_type);

                var nSize = samples * bytesPerSample;
                var byteArray = new byte[samples * bytesPerSample];
                Marshal.Copy(buffer, byteArray, 0, samples * bytesPerSample);

                var floatArray = ConvertByteToFloat16(byteArray);

                ActionQueue.Enqueue(() =>
                {
                    // logger.UpdateLog(string.Format("{0}", BitConverter.ToString(byteArray)));
                    logger.UpdateLog(string.Format("{0}", floatArray[0]));
                    var audioClip = AudioClip.Create("externalAudio", floatArray.Length, 1, SAMPLE_RATE, false);
                    audioClip.SetData(floatArray, 0);
                    AudioSource.PlayClipAtPoint(audioClip, Vector3.zero);
                    // var aud = GetComponent<AudioSource>();
                    // aud.clip = audioClip;
                    // aud.Play();
                });
                
                Marshal.FreeHGlobal(buffer);
            }).ContinueInMainThread(() =>
            {
                var action = ActionQueue.Dequeue();
                action();
            });
        }

        private static float[] ConvertByteToFloat16(byte[] byteArray)
        {
            var floatArray = new float[byteArray.Length / 2];
            for (var i = 0; i < floatArray.Length; i++)
            {
                if (BitConverter.IsLittleEndian)  Array.Reverse(byteArray, i * 2, 2);
                floatArray[i] = BitConverter.ToInt16(byteArray, i * 2) / 32767f;
            }

            return floatArray;
        }
    }
}