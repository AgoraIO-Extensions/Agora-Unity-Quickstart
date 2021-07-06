﻿// using System;
// using System.Runtime.InteropServices;
// using System.Threading;
// using UnityEngine;
// using UnityEngine.UI;
// using agora_gaming_rtc;
// using RingBuffer;
// using UnityEngine.Serialization;
//
// namespace CustomAudioSinkSample
// {
//     public class CustomAudioSinkSample : MonoBehaviour
//     {
//         [FormerlySerializedAs("APP_ID")] [SerializeField]
//         private string appID = "YOUR_APPID";
//
//         [FormerlySerializedAs("TOKEN")] [SerializeField]
//         private string token = "";
//
//         [FormerlySerializedAs("CHANNEL_NAME")] [SerializeField]
//         private string channelName = "YOUR_CHANNEL_NAME";
//
//         public Text logText;
//         internal Logger Logger;
//         internal AgoraRtcEngine AgoraRtcEngine;
//         private IAudioRawDataManager _audioRawDataManager;
//
//         private int CHANNEL = 1;
//         private int SAMPLE_RATE = 44100;
//         private int PULL_FREQ_PER_SEC = 100;
//
//         private int count;
//
//         private int writeCount;
//         private int readCount;
//
//         private RingBuffer<float> audioBuffer;
//         private AudioClip _audioClip;
//
//
//         private Thread _pullAudioFrameThread;
//         private bool _pullAudioFrameThreadSignal = true;
//
//         private bool _startSignal;
//
//         // Start is called before the first frame update
//         void Start()
//         {
//             var ifValid = CheckAppId();
//             InitRtcEngine();
//             JoinChannel();
//
//             var aud = GetComponent<AudioSource>();
//             if (aud == null)
//             {
//                 gameObject.AddComponent<AudioSource>();
//             }
//
//             if (ifValid) StartPullAudioFrame(aud, "externalClip");
//         }
//
//         void Update()
//         {
//             PermissionHelper.RequestMicrophontPermission();
//         }
//
//         bool CheckAppId()
//         {
//             Logger = new Logger(logText);
//             return Logger.DebugAssert(appID.Length > 10, "Please fill in your appId in Canvas!!!!!");
//         }
//
//         void InitRtcEngine()
//         {
//             AgoraRtcEngine = IRtcEngine.GetEngine(appID);
//             AgoraRtcEngine.SetExternalAudioSink(true, SAMPLE_RATE, CHANNEL);
//             AgoraRtcEngine.SetLogFile("log.txt");
//             AgoraRtcEngine.OnJoinChannelSuccess += OnJoinChannelSuccessHandler;
//             AgoraRtcEngine.OnLeaveChannel += OnLeaveChannelHandler;
//             AgoraRtcEngine.OnWarning += OnSDKWarningHandler;
//             AgoraRtcEngine.OnError += OnSDKErrorHandler;
//             AgoraRtcEngine.OnConnectionLost += OnConnectionLostHandler;
//         }
//
//         void JoinChannel()
//         {
//             AgoraRtcEngine.JoinChannelByKey(token, channelName, "", 0);
//         }
//
//         void StartPullAudioFrame(AudioSource aud, string clipName)
//         {
//             _audioRawDataManager = AudioRawDataManager.GetInstance(AgoraRtcEngine);
//
//             var bufferLength = SAMPLE_RATE / PULL_FREQ_PER_SEC * CHANNEL * 1000; // 10-sec-length buffer
//             audioBuffer = new RingBuffer<float>(bufferLength);
//
//             _pullAudioFrameThread = new Thread(PullAudioFrameThread);
//             _pullAudioFrameThread.Start();
//
//             _audioClip = AudioClip.Create(clipName,
//                 SAMPLE_RATE / PULL_FREQ_PER_SEC * CHANNEL, CHANNEL, SAMPLE_RATE, true,
//                 OnAudioRead);
//             aud.clip = _audioClip;
//             aud.loop = true;
//             aud.Play();
//         }
//
//         void OnApplicationQuit()
//         {
//             Debug.Log("OnApplicationQuit");
//             _pullAudioFrameThreadSignal = false;
//             _pullAudioFrameThread.Abort();
//             if (AgoraRtcEngine != null)
//             {
//                 IRtcEngine.Destroy();
//             }
//         }
//
//         void OnJoinChannelSuccessHandler(string channelName, uint uid, int elapsed)
//         {
//             Logger.UpdateLog(string.Format("sdk version: {0}", IRtcEngine.GetSdkVersion()));
//             Logger.UpdateLog(string.Format("onJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}", channelName,
//                 uid, elapsed));
//         }
//
//         void OnLeaveChannelHandler(RtcStats stats)
//         {
//             Logger.UpdateLog("OnLeaveChannelSuccess");
//         }
//
//         void OnSDKWarningHandler(int warn, string msg)
//         {
//             Logger.UpdateLog(string.Format("OnSDKWarning warn: {0}, msg: {1}", warn, msg));
//         }
//
//         void OnSDKErrorHandler(int error, string msg)
//         {
//             Logger.UpdateLog(string.Format("OnSDKError error: {0}, msg: {1}", error, msg));
//         }
//
//         void OnConnectionLostHandler()
//         {
//             Logger.UpdateLog(string.Format("OnConnectionLost "));
//         }
//
//         private void PullAudioFrameThread()
//         {
//             var avsync_type = 0;
//             var bytesPerSample = 2;
//             var type = AUDIO_FRAME_TYPE.FRAME_TYPE_PCM16;
//             var channels = CHANNEL;
//             var samples = SAMPLE_RATE / PULL_FREQ_PER_SEC * CHANNEL;
//             var samplesPerSec = SAMPLE_RATE;
//             var buffer = Marshal.AllocHGlobal(samples * bytesPerSample);
//             var freq = 1000 / PULL_FREQ_PER_SEC;
//
//             var tic = new TimeSpan(DateTime.Now.Ticks);
//
//             while (_pullAudioFrameThreadSignal)
//             {
//                 var toc = new TimeSpan(DateTime.Now.Ticks);
//                 if (toc.Subtract(tic).Duration().Milliseconds >= freq)
//                 {
//                     tic = new TimeSpan(DateTime.Now.Ticks);
//                     _audioRawDataManager.PullAudioFrame(buffer, (int) type, samples, bytesPerSample, channels,
//                         samplesPerSec, 0, avsync_type);
//
//                     var byteArray = new byte[samples * bytesPerSample];
//                     Marshal.Copy(buffer, byteArray, 0, samples * bytesPerSample);
//
//                     var floatArray = ConvertByteToFloat16(byteArray);
//                     lock (audioBuffer)
//                     {
//                         audioBuffer.Put(floatArray);
//                     }
//
//                     writeCount += floatArray.Length;
//                     count += 1;
//                 }
//
//                 if (count == 100)
//                 {
//                     _startSignal = true;
//                 }
//             }
//
//             Marshal.FreeHGlobal(buffer);
//         }
//
//         private static float[] ConvertByteToFloat16(byte[] byteArray)
//         {
//             var floatArray = new float[byteArray.Length / 2];
//             for (var i = 0; i < floatArray.Length; i++)
//             {
//                 floatArray[i] = BitConverter.ToInt16(byteArray, i * 2) / 32768f; // -Int16.MinValue
//             }
//
//             return floatArray;
//         }
//
//
//         private void OnAudioRead(float[] data)
//         {
//             if (!_startSignal) return;
//             for (var i = 0; i < data.Length; i++)
//             {
//                 lock (audioBuffer)
//                 {
//                     data[i] = audioBuffer.Get();
//                 }
//
//                 readCount += 1;
//             }
//
//             Debug.LogFormat("buffer length remains: {0}", writeCount - readCount);
//         }
//     }
// }