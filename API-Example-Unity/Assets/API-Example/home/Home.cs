using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using agora.rtc;
using agora.util;
using UnityEngine.SceneManagement;
using Logger = agora.util.Logger;


public class Home : MonoBehaviour
{
    public AgoraBaseProfile profile;
    //private IAgoraRtcEngine _mRtcEngine = null;

    private string PlaySceneName = "BasicVideoCall";
    
    private void Awake()
    {
#if (UNITY_2018_3_OR_NEWER && UNITY_ANDROID)
        PermissionHelper.RequestMicrophontPermission();
        PermissionHelper.RequestCameraPermission();
#endif
    }

    // Start is called before the first frame update
    private void Start()
    {
        profile.isHomeStart = true;
        //_mRtcEngine = AgoraRtcEngine.CreateAgoraRtcEngine();
    }

    // Update is called once per frame
    private void Update()
    {
        
    }

    private void OnApplicationQuit()
    {
        Debug.Log("OnApplicationQuit");
        profile.isHomeStart = false;
        // if (_mRtcEngine == null) return;
        // _mRtcEngine.Dispose();
    }


    public void JoinBasicVideoCallScene()
    {
        SceneManager.LoadScene("BasicVideoCallScene", LoadSceneMode.Single);
    }

    public void JoinBasicAudioCallScene()
    {
        SceneManager.LoadScene("BasicAudioCallScene", LoadSceneMode.Single);
    }

    public void JoinAudioMixingScene()
    {
        SceneManager.LoadScene("AudioMixingScene", LoadSceneMode.Single);
    }

    public void JoinScreenShareScene()
    {
        SceneManager.LoadScene("ScreenShareScene", LoadSceneMode.Single);
    }

    public void JoinMediaPlayerScene()
    {
        SceneManager.LoadScene("MediaPlayerScene", LoadSceneMode.Single);
    }

    public void JoinVoiceChangerScene()
    {
        SceneManager.LoadScene("VoiceChangerScene", LoadSceneMode.Single);
    }

    public void JoinRtmpStreamingScene()
    {
        SceneManager.LoadScene("RtmpStreamingScene", LoadSceneMode.Single);
    }

    public void JoinDeviceManagerScene()
    {
        SceneManager.LoadScene("DeviceManagerScene", LoadSceneMode.Single);
    }
}
