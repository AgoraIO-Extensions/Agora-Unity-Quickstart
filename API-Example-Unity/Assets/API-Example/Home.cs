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
    public InputField AppIdInupt;
    public InputField ChannelInput;
    public InputField TokenInput;


    public AppIdInput AppInputConfig;
    public GameObject CasePanel;
    public GameObject CaseScrollerView;

    private string _playSceneName = "";


    private string[] _baseSceneNameList = {
        "BasicAudioCallScene",
        "BasicVideoCallScene"
    };

    private string[] _advancedNameList = {
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

        for (int i = 0; i < _baseSceneNameList.Length; i++)
        {
            var go = Instantiate(CasePanel, content.transform);
            var name = go.transform.Find("Text").gameObject.GetComponent<Text>();
            name.text = _baseSceneNameList[i];
            var button = go.transform.Find("Button").gameObject.GetComponent<Button>();
            button.onClick.AddListener(OnJoinSceneClicked);
            button.onClick.AddListener(SetScolllerActive);
        }

        for (int i = 0; i < _advancedNameList.Length; i++)
        {
            var go = Instantiate(CasePanel, content.transform);
            var name = go.transform.Find("Text").gameObject.GetComponent<Text>();
            name.text = _advancedNameList[i];
            var button = go.transform.Find("Button").gameObject.GetComponent<Button>();
            button.onClick.AddListener(OnJoinSceneClicked);
            button.onClick.AddListener(SetScolllerActive);
        }


        if (this.AppInputConfig)
        {
            this.AppIdInupt.text = this.AppInputConfig.appID;
            this.ChannelInput.text = this.AppInputConfig.channelName;
            this.TokenInput.text = this.AppInputConfig.token;
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
    }

    public void OnLeaveButtonClicked()
    {
        StartCoroutine(UnloadSceneAsync());
        CaseScrollerView.SetActive(true);
    }

    public IEnumerator UnloadSceneAsync()
    {
        if (this._playSceneName != "")
        {
            AsyncOperation async = SceneManager.UnloadSceneAsync(_playSceneName);
            yield return async;
        }
    }

    public void OnJoinSceneClicked()
    {
        this.AppInputConfig.appID = this.AppIdInupt.text;
        this.AppInputConfig.channelName = this.ChannelInput.text;
        this.AppInputConfig.token = this.TokenInput.text;

        var button = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
        var sceneName = button.transform.parent.Find("Text").gameObject.GetComponent<Text>().text;

        SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
        this._playSceneName = sceneName;
    }

    public void SetScolllerActive()
    {
        CaseScrollerView.SetActive(false);
    }
}
