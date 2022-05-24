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
    public InputField appIdInupt;
    public InputField channelInput;
    public InputField tokenInput;


    public AppIdInput appIdInput;
    public GameObject casePanel;
    public GameObject caseScoller;
    private string PlaySceneName = "";


    private string[] baseSceneNameList = {
        "BasicAudioCallScene",
        "BasicVideoCallScene"
    };

    private string[] advancedNameList = {
        "AudioMixingScene",
        "ChannelMediaRelayScene",
        "CustomCaptureAudioScene",
        "CustomCaptureVideoScene",
        "CustomRenderAudioScene",
        "DeviceManagerScene",
        "DualCameraScene",
        "JoinChannelVideoTokenScene",
        "JoinChannelWithUserAccountScene",
        "MediaPlayerScene",
        "ProcessRawDataScene",
        "PushEncodedVideoImageScene",
        "RtmpStreamingScene",
        "ScreenShareScene",
        "ScreenShareWhileVideoCallScene",
        "SetEncryptionScene",
        "SetVideoEncodeConfigurationScene",
        "SpatialAudioWithMediaPlayerScene",
        "StartRhythmPlayerScene",
        "StreamMessageScene",
        "TakeSnapshotScene",
        "VoiceChangerScene"
    };



    private void Awake()
    {
#if (UNITY_2018_3_OR_NEWER && UNITY_ANDROID)
        PermissionHelper.RequestMicrophontPermission();
        PermissionHelper.RequestCameraPermission();
#endif

        GameObject content = GameObject.Find("Content");
        var contentRectTrans = content.GetComponent<RectTransform>();

        for (int i = 0; i < baseSceneNameList.Length; i++)
        {
            var go = Instantiate(casePanel, content.transform);
            var name = go.transform.Find("Text").gameObject.GetComponent<Text>();
            name.text = baseSceneNameList[i];
            var button = go.transform.Find("Button").gameObject.GetComponent<Button>();
            button.onClick.AddListener(OnJoinSceneClicked);
            button.onClick.AddListener(SetScolllerActive);
        }

        for (int i = 0; i < advancedNameList.Length; i++)
        {
            var go = Instantiate(casePanel, content.transform);
            var name = go.transform.Find("Text").gameObject.GetComponent<Text>();
            name.text = advancedNameList[i];
            var button = go.transform.Find("Button").gameObject.GetComponent<Button>();
            button.onClick.AddListener(OnJoinSceneClicked);
            button.onClick.AddListener(SetScolllerActive);
        }


        if (this.appIdInput)
        {
            this.appIdInupt.text = this.appIdInput.appID;
            this.channelInput.text = this.appIdInput.channelName;
            this.tokenInput.text = this.appIdInput.token;
        }

    }

    // Start is called before the first frame update
    private void Start()
    {

    }

    // Update is called once per frame
    private void Update()
    {

    }

    private void OnApplicationQuit()
    {
        Debug.Log("OnApplicationQuit");
        IRtcEngine mRtcEngine = RtcEngineImpl.Get();
        if (mRtcEngine != null)
        {
            mRtcEngine.Dispose(true);
        }
    }

    public void OnLeaveButtonClicked()
    {
        StartCoroutine(UnloadSceneAsync());
        caseScoller.SetActive(true);
    }

    public IEnumerator UnloadSceneAsync()
    {
        if (this.PlaySceneName != "")
        {
            AsyncOperation async = SceneManager.UnloadSceneAsync(PlaySceneName);
            yield return async;
        }
    }

    public void OnJoinSceneClicked()
    {
        this.appIdInput.appID = this.appIdInupt.text;
        this.appIdInput.channelName = this.channelInput.text;
        this.appIdInput.token = this.tokenInput.text;

        var button = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
        var sceneName = button.transform.parent.Find("Text").gameObject.GetComponent<Text>().text;

        SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
        this.PlaySceneName = sceneName;
    }

    public void SetScolllerActive()
    {
        caseScoller.SetActive(false);
    }
}
