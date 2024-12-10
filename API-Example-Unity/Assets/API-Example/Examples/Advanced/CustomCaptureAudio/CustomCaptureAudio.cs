using System;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using System.Runtime.InteropServices;
using Agora.Rtc;

using RingBuffer;
using io.agora.rtc.demo;
using System.IO;
using System.Collections.Generic;
using UnityEngine.Analytics;
using UnityEngine.Networking;
using System.Collections;

namespace Agora_RTC_Plugin.API_Example.Examples.Advanced.CustomCaptureAudio
{
    public class CustomCaptureAudio : MonoBehaviour
    {
        [FormerlySerializedAs("appIdInput")]
        [SerializeField]
        private AppIdInput _appIdInput;

        [Header("_____________Basic Configuration_____________")]
        [FormerlySerializedAs("APP_ID")]
        [SerializeField]
        private string _appID = "";

        [FormerlySerializedAs("TOKEN")]
        [SerializeField]
        private string _token = "";

        [FormerlySerializedAs("CHANNEL_NAME")]
        [SerializeField]
        private string _channelName = "";

        public Text LogText;
        internal Logger Log;
        internal IRtcEngine RtcEngine = null;
        internal uint AUDIO_TRACK_ID = 0;
       

        // Number of push audio frame per second.
        private const int PUSH_FREQ_PER_SEC = 20;

        private Thread _pushAudioFrameThread;
        private System.Object _rtcLock = new System.Object();



        // Use this for initialization
        private void Start()
        {
            LoadAssetData();
            if (CheckAppId())
            {
                InitRtcEngine();
                CreateCustomAudioSource();
                JoinChannel();
                StartPushAudioFrame();
            }
        }

        // Update is called once per frame
        private void Update()
        {
            PermissionHelper.RequestMicrophontPermission();
        }

        //Show data in AgoraBasicProfile
        [ContextMenu("ShowAgoraBasicProfileData")]
        private void LoadAssetData()
        {
            if (_appIdInput == null) return;
            _appID = _appIdInput.appID;
            _token = _appIdInput.token;
            _channelName = _appIdInput.channelName;
        }

        private bool CheckAppId()
        {
            Log = new Logger(LogText);
            return Log.DebugAssert(_appID.Length > 10, "Please fill in your appId in API-Example/profile/appIdInput.asset");
        }

        private void InitRtcEngine()
        {
            lock (_rtcLock)
            {

                RtcEngine = Agora.Rtc.RtcEngine.CreateAgoraRtcEngine();

                RtcEngineContext context = new RtcEngineContext();
                context.appId = _appID;
                context.channelProfile = CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING;
                context.audioScenario = AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT;
                context.areaCode = AREA_CODE.AREA_CODE_GLOB;

                RtcEngine.Initialize(context);
                RtcEngine.InitEventHandler(new UserEventHandler(this));
            }
        }

        private void CreateCustomAudioSource()
        {
            lock (_rtcLock)
            {
                AudioTrackConfig audioTrackConfig = new AudioTrackConfig(true, false);
                AUDIO_TRACK_ID = RtcEngine.CreateCustomAudioTrack(AUDIO_TRACK_TYPE.AUDIO_TRACK_MIXABLE, audioTrackConfig);
                this.Log.UpdateLog("CreateCustomAudioTrack id:" + AUDIO_TRACK_ID);
            }
        }

        private IEnumerator PreparationFilePath(Action<string> callback)
        {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR          
            string fromPath = Path.Combine(Application.streamingAssetsPath, "audio/pcm16.wav");
            string filePath = Path.Combine(Application.persistentDataPath, "pcm16.wav");
            if (fromPath.Contains("://") || fromPath.Contains(":///"))
            {
                using (UnityWebRequest www = UnityWebRequest.Get(fromPath))
                {
                    yield return www.SendWebRequest();


                    if (www.result != UnityWebRequest.Result.Success)
                    {
                        Debug.LogError("Failed to load file: " + www.error);
                    }
                    else
                    {

                        try
                        {
                            File.WriteAllBytes(filePath, www.downloadHandler.data);
                            Debug.Log("File successfully copied to " + filePath);
                        }
                        catch (Exception e)
                        {
                            Debug.LogError("Failed to save file: " + e.Message);
                        }
                    }
                }
            }
            else
            {
                try
                {
                    File.Copy(fromPath, filePath, true);
                    Debug.Log("File successfully copied to " + filePath);
                }
                catch (Exception e)
                {
                    Debug.LogError("Failed to copy file: " + e.Message);
                }
            }
#else
            string filePath = Path.Combine(Application.streamingAssetsPath, "audio/pcm16.wav");
#endif

            callback(filePath);
            yield break;
        }


        private void StartPushAudioFrame()
        {
            Action<string> action = (filePath) =>
            {
                ParameterizedThreadStart threadStart = new ParameterizedThreadStart(PushAudioFrameThread);
                _pushAudioFrameThread = new Thread(threadStart);
                _pushAudioFrameThread.Start(filePath);
            };
            this.Log.UpdateLog("Because the api of rtcEngine is called in different threads, it is necessary to use locks to ensure that different threads do not call the api of rtcEngine at the same time");
            StartCoroutine(PreparationFilePath(action));
        }

        private void JoinChannel()
        {
            lock (_rtcLock)
            {
                RtcEngine.SetAudioProfile(AUDIO_PROFILE_TYPE.AUDIO_PROFILE_MUSIC_HIGH_QUALITY,
                AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT);
                RtcEngine.EnableAudio();
                RtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
                ChannelMediaOptions channelMediaOptions = new ChannelMediaOptions();
                channelMediaOptions.publishCustomAudioTrack.SetValue(true);
                channelMediaOptions.publishCustomAudioTrackId.SetValue((int)AUDIO_TRACK_ID);
                channelMediaOptions.publishMicrophoneTrack.SetValue(false);

                RtcEngine.JoinChannel(_token, _channelName, 0, channelMediaOptions);
            }
        }

        private void OnLeaveBtnClick()
        {
            RtcEngine.LeaveChannel();
        }

        private void OnDestroy()
        {
            Debug.Log("OnDestroy");

            lock (_rtcLock)
            {
                if (RtcEngine == null) return;
                RtcEngine.InitEventHandler(null);
                RtcEngine.LeaveChannel();
                RtcEngine.DestroyCustomAudioTrack(AUDIO_TRACK_ID);
                RtcEngine.Dispose();
                RtcEngine = null;
            }
            //need wait pullAudioFrameThread stop 
            _pushAudioFrameThread.Join();

        }


        private bool ReadAudioHeaderAndDetectFormat(FileStream fileStream, AudioFrame audioFrame)
        {
            // Ensure that the file has at least 44 bytes (the minimum size for WAV file headers)
            if (fileStream.Length < 44)
            {
                Debug.LogError("File is too short to be a valid WAV file.");
                return false;
            }

            // Read RIFF header
            byte[] riffHeader = new byte[4];
            fileStream.Read(riffHeader, 0, 4);
            string riff = System.Text.Encoding.ASCII.GetString(riffHeader);
            if (riff != "RIFF")
            {
                Debug.LogError("Not a valid WAV file.");
                return false;
            }

            // Read file size (excluding header information of 8 bytes)
            byte[] fileSizeBytes = new byte[4];
            fileStream.Read(fileSizeBytes, 0, 4);
            int fileSize = BitConverter.ToInt32(fileSizeBytes, 0);

            // Read WAVE tag
            byte[] waveHeader = new byte[4];
            fileStream.Read(waveHeader, 0, 4);
            string wave = System.Text.Encoding.ASCII.GetString(waveHeader);
            if (wave != "WAVE")
            {
                Debug.LogError("Not a valid WAV file.");
                return false;
            }

            // Read format block tag 'fmt '
            byte[] fmtHeader = new byte[4];
            fileStream.Read(fmtHeader, 0, 4);
            string fmt = System.Text.Encoding.ASCII.GetString(fmtHeader);
            if (fmt != "fmt ")
            {
                Debug.LogError("Unsupported format chunk.");
                return false;
            }

            // Read format block size
            byte[] fmtSizeBytes = new byte[4];
            fileStream.Read(fmtSizeBytes, 0, 4);
            int fmtSize = BitConverter.ToInt32(fmtSizeBytes, 0);

            // Read audio format (PCM=1)
            byte[] audioFormatBytes = new byte[2];
            fileStream.Read(audioFormatBytes, 0, 2);
            int audioFormat = BitConverter.ToInt16(audioFormatBytes, 0);
            if (audioFormat != 1)
            {
                Debug.LogError("Unsupported audio format.");
                return false;
            }

            // Read the number of channels
            byte[] channelsBytes = new byte[2];
            fileStream.Read(channelsBytes, 0, 2);
            int channels = BitConverter.ToInt16(channelsBytes, 0);

            // Read sampling rate
            byte[] sampleRateBytes = new byte[4];
            fileStream.Read(sampleRateBytes, 0, 4);
            int sampleRate = BitConverter.ToInt32(sampleRateBytes, 0);

            // Read byte rate (data transfer rate)
            byte[] byteRateBytes = new byte[4];
            fileStream.Read(byteRateBytes, 0, 4);
            int byteRate = BitConverter.ToInt32(byteRateBytes, 0);

            // Read block alignment
            byte[] blockAlignBytes = new byte[2];
            fileStream.Read(blockAlignBytes, 0, 2);
            int blockAlign = BitConverter.ToInt16(blockAlignBytes, 0);

            // Read bit depth
            byte[] bitsPerSampleBytes = new byte[2];
            fileStream.Read(bitsPerSampleBytes, 0, 2);
            int bitsPerSample = BitConverter.ToInt16(bitsPerSampleBytes, 0);

            Debug.Log($"Format: PCM");
            Debug.Log($"Channels: {channels}");
            Debug.Log($"Sample Rate: {sampleRate} Hz");
            Debug.Log($"Byte Rate: {byteRate} bps");
            Debug.Log($"Block Align: {blockAlign}");
            Debug.Log($"Bits Per Sample: {bitsPerSample} bits");


            audioFrame.bytesPerSample = BYTES_PER_SAMPLE.TWO_BYTES_PER_SAMPLE;
            audioFrame.type = AUDIO_FRAME_TYPE.FRAME_TYPE_PCM16;
            audioFrame.samplesPerSec = sampleRate;
            audioFrame.samplesPerChannel = sampleRate / PUSH_FREQ_PER_SEC;
            audioFrame.channels = channels;
            audioFrame.renderTimeMs = 0;
            audioFrame.RawBuffer = new byte[audioFrame.samplesPerChannel * (int)BYTES_PER_SAMPLE.TWO_BYTES_PER_SAMPLE * channels];
            return true;
        }


        private void SeekToAudioData(FileStream fileStream)
        {
            byte[] buffer = new byte[4];
            fileStream.Seek(0, SeekOrigin.Begin);
            if (fileStream.Read(buffer, 0, 4) != 4)
            {
                throw new Exception("Failed to read RIFF header.");
            }
            string riff = System.Text.Encoding.UTF8.GetString(buffer);
            if (riff != "RIFF")
            {
                throw new Exception("Not a valid WAV file.");
            }


            if (fileStream.Read(buffer, 0, 4) != 4)
            {
                throw new Exception("Failed to read file size.");
            }
            int fileSize = BitConverter.ToInt32(buffer, 0);

            if (fileStream.Read(buffer, 0, 4) != 4)
            {
                throw new Exception("Failed to read WAVE marker.");
            }
            string wave = System.Text.Encoding.UTF8.GetString(buffer);
            if (wave != "WAVE")
            {
                throw new Exception("Not a valid WAV file.");
            }

            while (fileStream.Position < fileSize + 8)
            {
                if (fileStream.Read(buffer, 0, 4) != 4)
                {
                    throw new Exception("Failed to read chunk ID.");
                }
                string chunkId = System.Text.Encoding.UTF8.GetString(buffer);
                if (fileStream.Read(buffer, 0, 4) != 4)
                {
                    throw new Exception("Failed to read chunk size.");
                }
                int chunkSize = BitConverter.ToInt32(buffer, 0);

                if (chunkId == "data")
                {
                    Debug.Log("Audio data chunk found at position: " + fileStream.Position);
                    return;
                }
                else
                {
                    fileStream.Seek(chunkSize, SeekOrigin.Current);
                }
            }

        }

        private void PushAudioFrameThread(object file)
        {

            string filePath = (string)file;
            FileStream fileStream = new FileStream(filePath, FileMode.Open);
            var audioFrame = new AudioFrame();
            if (ReadAudioHeaderAndDetectFormat(fileStream, audioFrame) == false)
            {
                return;
            }

            var freq = 1000 / PUSH_FREQ_PER_SEC;

            SeekToAudioData(fileStream);
            double startMillisecond = GetTimestamp();
            long tick = 0;

            while (true)
            {
                int nRet = -1;
                lock (_rtcLock)
                {
                    if (RtcEngine == null)
                    {
                        break;
                    }

                    int bytesRead = fileStream.Read(audioFrame.RawBuffer, 0, audioFrame.RawBuffer.Length);
                    nRet = RtcEngine.PushAudioFrame(audioFrame, AUDIO_TRACK_ID);

                    if (bytesRead == 0)
                    {
                        //Set the position of FileStream to return to the file header to restart reading data
                        SeekToAudioData(fileStream);
                    }
                }

                if (nRet == 0)
                {
                    tick++;
                    double nextMillisecond = startMillisecond + tick * freq;
                    double curMillisecond = GetTimestamp();
                    int sleepMillisecond = (int)Math.Ceiling(nextMillisecond - curMillisecond);
                    Debug.Log("sleepMillisecond : " + sleepMillisecond);
                    if (sleepMillisecond > 0)
                    {
                        Thread.Sleep(sleepMillisecond);
                    }
                    else
                    {
                        Debug.Log("Sleep 1ms--1");
                        Thread.Sleep(1);
                    }
                }
                else
                {
                    Debug.Log("Sleep freq");
                    Thread.Sleep(freq);
                    startMillisecond = GetTimestamp();
                    tick = 0;

                }
            }
            fileStream.Close();
        }

        //get timestamp millisecond
        private double GetTimestamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return ts.TotalMilliseconds;
        }
    }

    #region -- Agora Event ---

    internal class UserEventHandler : IRtcEngineEventHandler
    {
        private readonly CustomCaptureAudio _customAudioSource;

        internal UserEventHandler(CustomCaptureAudio customAudioSource)
        {
            _customAudioSource = customAudioSource;
        }

        public override void OnJoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            int build = 0;
            _customAudioSource.Log.UpdateLog(string.Format("sdk version: {0}",
                _customAudioSource.RtcEngine.GetVersion(ref build)));
            _customAudioSource.Log.UpdateLog(string.Format(
                "onJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}", connection.channelId,
                connection.localUid, elapsed));
        }

        public override void OnLeaveChannel(RtcConnection connection, RtcStats stats)
        {
            _customAudioSource.Log.UpdateLog("OnLeaveChannelSuccess");
        }

        public override void OnError(int error, string msg)
        {
            _customAudioSource.Log.UpdateLog(string.Format("OnSDKError error: {0}, msg: {1}", error, msg));
        }

        public override void OnConnectionLost(RtcConnection connection)
        {
            _customAudioSource.Log.UpdateLog(string.Format("OnConnectionLost "));
        }
    }

    #endregion
}