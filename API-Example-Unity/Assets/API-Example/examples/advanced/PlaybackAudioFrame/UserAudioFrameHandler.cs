using System;
using UnityEngine;
using agora_gaming_rtc;
using RingBuffer;

public class UserAudioFrameHandler : MonoBehaviour {

	AudioSource audioSource;
    IUserAudioFrameDelegate _userAudioFrameDelegate;

    uint UID { get; set; }

    private int CHANNEL = 2;
    private int PULL_FREQ_PER_SEC = 100;
    private int SAMPLE_RATE = 48000; // this should = CLIP_SAMPLES x PULL_FREQ_PER_SEC
    private int CLIP_SAMPLES = 480;

    private int count;

    private int writeCount;
    private int readCount;

    private RingBuffer<float> audioBuffer;
    private AudioClip _audioClip = null;

    private bool _startSignal;


    // Use this for initialization (runs after Init)
    void Start () {
		audioSource = GetComponent<AudioSource>();	
        if (audioSource == null)
		{
			audioSource = gameObject.AddComponent<AudioSource>();
	    }
        _userAudioFrameDelegate.HandleAudioFrameForUser += HandleAudioFrame;
        SetupAudio(audioSource, "clip_for_" + UID);
    }

    private void OnDisable()
    {
        _userAudioFrameDelegate.HandleAudioFrameForUser -= HandleAudioFrame;
        ResetHandler();
    }

    public void Init(uint uid, IUserAudioFrameDelegate userAudioFrameDelegate, AudioFrame audioFrame)
    {
        Debug.Log("INIT:" + uid + " audioFrame:" + audioFrame);
        _userAudioFrameDelegate = userAudioFrameDelegate;
        UID = uid;
        CLIP_SAMPLES = audioFrame.samples;
        SAMPLE_RATE = audioFrame.samplesPerSec;
        CHANNEL = audioFrame.channels;
    }

    void SetupAudio(AudioSource aud, string clipName)
    {
        if (_audioClip != null) return; 
        var bufferLength = SAMPLE_RATE / PULL_FREQ_PER_SEC * CHANNEL * 1000; // 10-sec-length buffer
        audioBuffer = new RingBuffer<float>(bufferLength);
        // Debug.Log($"{UID} Created clip for SAMPLE_RATE:" + SAMPLE_RATE + " CLIP_SAMPLES:" + CLIP_SAMPLES + " channel:" + CHANNEL + " => bufferLength = " + bufferLength);
        _audioClip = AudioClip.Create(clipName,
            CLIP_SAMPLES,
            CHANNEL, SAMPLE_RATE, true,
            OnAudioRead);
        aud.clip = _audioClip;
        aud.loop = true;
        aud.Play();
    }

    void ResetHandler()
    {
        _startSignal = false;
        if (audioBuffer != null)
        {
            audioBuffer.Clear();
        }
        count = 0;
    }

    private void OnApplicationPause(bool pause)
    {
        Debug.LogWarning("Pausing application");
        if (pause)
        {
            ResetHandler();
        }
        else
        {
            Debug.Log("Application resumed.");
        }
    }

    void HandleAudioFrame(uint uid, AudioFrame audioFrame)
    {
        if (UID != uid || audioBuffer == null) return;

        var floatArray = ConvertByteToFloat16(audioFrame.buffer);
        lock (audioBuffer)
        {
            audioBuffer.Put(floatArray);
            writeCount += floatArray.Length;
            count++;
        }

        if (count == 100)
        {
            _startSignal = true;
        }
    }

    private void OnAudioRead(float[] data)
    {
        if (!_startSignal) return;
        for (var i = 0; i < data.Length; i++)
        {
            lock (audioBuffer)
            {
                data[i] = audioBuffer.Get();
                readCount += 1;
            }
        }
       // Debug.Log("buffer length remains: {0}", writeCount - readCount);
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
}
