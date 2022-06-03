using System;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;
using agora_gaming_rtc;
using RingBuffer;

namespace agora_sample
{
    public class CustomAudioSinkPlayer : MonoBehaviour
    {
        private IRtcEngine mRtcEngine = null;
        private IAudioRawDataManager _audioRawDataManager;

        public int CHANNEL = 1;
        public int SAMPLE_RATE = 44100;
        public int PULL_FREQ_PER_SEC = 100;
        public bool DebugFlag = false;

        const int BYTES_PER_SAMPLE = 2;

        int SAMPLES;
        int FREQ;
        int BUFFER_SIZE;

        private int writeCount = 0;
        private int readCount = 0;

        private RingBuffer<float> audioBuffer;
        private AudioClip _audioClip;


        private Thread _pullAudioFrameThread = null;
        private bool _pullAudioFrameThreadSignal = true;

        IntPtr BufferPtr { get; set; }

        // Start is called before the first frame update
        void Start()
        {
            SAMPLES = SAMPLE_RATE / PULL_FREQ_PER_SEC * CHANNEL;
            FREQ = 1000 / PULL_FREQ_PER_SEC;
            BUFFER_SIZE = SAMPLES * BYTES_PER_SAMPLE;

            StartCoroutine(CoStartRunning());
        }

        System.Collections.IEnumerator CoStartRunning()
        {
            while (mRtcEngine == null)
            {
                yield return new WaitForFixedUpdate();
                mRtcEngine = IRtcEngine.QueryEngine();
            }

            var aud = GetComponent<AudioSource>();
            if (aud == null)
            {
                aud = gameObject.AddComponent<AudioSource>();
            }
            KickStartAudio(aud, "externalClip");
        }

        void KickStartAudio(AudioSource aud, string clipName)
        {
            var bufferLength = SAMPLES * 100; // 1-sec-length buffer
            audioBuffer = new RingBuffer<float>(bufferLength, overflow: true);

            _audioRawDataManager = AudioRawDataManager.GetInstance(mRtcEngine);

            // Create and start the AudioClip playback, OnAudioRead will feed it
            _audioClip = AudioClip.Create(clipName,
                SAMPLE_RATE / PULL_FREQ_PER_SEC * CHANNEL, CHANNEL, SAMPLE_RATE, true,
                OnAudioRead);
            aud.clip = _audioClip;
            aud.loop = true;
            aud.Play();

            StartPullAudioThread();
        }

        void StartPullAudioThread()
        {
            if (_pullAudioFrameThread != null)
            {
                Debug.LogWarning("Stopping previous thread");
                _pullAudioFrameThread.Abort();
            }

            _pullAudioFrameThread = new Thread(PullAudioFrameThread);
            //_pullAudioFrameThread.Start("pullAudio" + writeCount);
            _pullAudioFrameThread.Start();
        }

        bool _paused = false;
        private void OnApplicationPause(bool pause)
        {
            if (pause)
            {
                if (DebugFlag)
                {
                    Debug.Log("Application paused. AudioBuffer length = " + audioBuffer.Size);
                    Debug.Log("PullAudioFrameThread state = " + _pullAudioFrameThread.ThreadState + " signal =" + _pullAudioFrameThreadSignal);
                }

                // Invalidate the buffer
                _pullAudioFrameThread.Abort();
                _pullAudioFrameThread = null;
                _paused = true;
            }
            else
            {
                if (_paused) // had been paused, not from starting up
                {
                    Debug.Log("Resuming PullAudioThread");
                    audioBuffer.Clear();
                    StartPullAudioThread();
                }
            }
        }


        void OnApplicationQuit()
        {
            Debug.Log("OnApplicationQuit");
            _pullAudioFrameThreadSignal = false;
            audioBuffer.Clear();
            if (BufferPtr != IntPtr.Zero)
            {
                Debug.LogWarning("cleanning up IntPtr buffer");
                Marshal.FreeHGlobal(BufferPtr);
                BufferPtr = IntPtr.Zero;
            }
            if (mRtcEngine != null)
            {
                IRtcEngine.Destroy();
            }
        }

        private void PullAudioFrameThread()
        {
            BufferPtr = Marshal.AllocHGlobal(BUFFER_SIZE);

            var tic = new TimeSpan(DateTime.Now.Ticks);

            var byteArray = new byte[BUFFER_SIZE];

            while (_pullAudioFrameThreadSignal)
            {
                var toc = new TimeSpan(DateTime.Now.Ticks);
                if (toc.Subtract(tic).Duration().Milliseconds >= FREQ)
                {
                    tic = new TimeSpan(DateTime.Now.Ticks);
                    int rc = _audioRawDataManager.PullAudioFrame(BufferPtr,
                        type: (int)AUDIO_FRAME_TYPE.FRAME_TYPE_PCM16,
                     samples: SAMPLES,
              bytesPerSample: 2,
                    channels: CHANNEL,
               samplesPerSec: SAMPLE_RATE,
                renderTimeMs: 0,
                 avsync_type: 0);

                    if (rc < 0)
                    {
                        Debug.LogWarning("PullAudioFrame returns " + rc);
                        continue;
                    }

                    Marshal.Copy(BufferPtr, byteArray, 0, BUFFER_SIZE);

                    var floatArray = ConvertByteToFloat16(byteArray);
                    lock (audioBuffer)
                    {
                        audioBuffer.Put(floatArray);
                    }

                    writeCount += floatArray.Length;
                    if (DebugFlag) Debug.Log("PullAudioFrame rc = " + rc + " writeCount = " + writeCount);
                }

            }

            if (BufferPtr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(BufferPtr);
                BufferPtr = IntPtr.Zero;
            }

            Debug.Log("Done running pull audio thread");
        }

        private static float[] ConvertByteToFloat16(byte[] byteArray)
        {
            var floatArray = new float[byteArray.Length / 2];
            for (var i = 0; i < floatArray.Length; i++)
            {
                floatArray[i] = BitConverter.ToInt16(byteArray, i * 2) / 32768f; // -Int16.MinValue
            }

            return floatArray;
        }

        // This Monobehavior method feeds data into the audio source
        private void OnAudioRead(float[] data)
        {
            for (var i = 0; i < data.Length; i++)
            {
                lock (audioBuffer)
                {
                    if (audioBuffer.Count > 0)
                    {
                        data[i] = audioBuffer.Get();
                    }
                    else
                    {
                        // no data
                        data[i] = 0;
                    }
                }

                readCount += 1;
            }

            if (DebugFlag)
            {
                Debug.LogFormat("buffer length remains: {0}", writeCount - readCount);
            }
        }
    }
}
