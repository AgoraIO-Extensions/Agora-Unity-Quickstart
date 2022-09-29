using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Agora.Util;

public class Home : MonoBehaviour
{
    public InputField AppIdInupt;
    public InputField ChannelInput;
    public InputField TokenInput;

    public AppIdInput AppInputConfig;
    public GameObject CasePanel;
    public GameObject CaseScrollerView;

    public GameObject EventSystem;

    private string _playSceneName = "";


    private string[] _baseSceneNameList = {
        "BasicAudioCallScene",
        "BasicVideoCallScene"
    };

    private string[] _advancedNameList = {
        "AudioMixingScene",
        "AudioSpectrumScene",
        "ChannelMediaRelayScene",
        "ContentInspectScene",
        "CustomCaptureAudioScene",
        "CustomCaptureVideoScene",
        "CustomRenderAudioScene",
        "DeviceManagerScene",
        "DualCameraScene",
        "JoinChannelVideoTokenScene",
        "JoinChannelWithUserAccountScene",
        "MediaPlayerScene",
        "MediaPlayerWithCustomDataProviderScene",
        "MediaRecorderScene",
        "MetadataScene",
        "ProcessAudioRawDataScene",
        "ProcessVideoRawDataScene",
        "PushEncodedVideoImageScene",
        "ScreenShareScene",
        "ScreenShareWhileVideoCallScene",
        "SetBeautyEffectOptionsScene",
        "SetEncryptionScene",
        "SetVideoEncodeConfigurationScene",
        "StartLocalVideoTranscoderScene",
        "SpatialAudioWithMediaPlayerScene",
        "StartDirectCdnStreamingScene",
        "StartRhythmPlayerScene",
        //"StartRtmpStreamWithTranscodingScene",
        "StreamMessageScene",
        "TakeSnapshotScene",
        "VirtualBackgroundScene",
        "VoiceChangerScene"
    };

    private void Awake()
    {
        PermissionHelper.RequestMicrophontPermission();
        PermissionHelper.RequestCameraPermission();

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
            EventSystem.gameObject.SetActive(true);
        }
    }

    public void OnJoinSceneClicked()
    {
        this.AppInputConfig.appID = this.AppIdInupt.text;
        this.AppInputConfig.channelName = this.ChannelInput.text;
        this.AppInputConfig.token = this.TokenInput.text;

        var button = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
        var sceneName = button.transform.parent.Find("Text").gameObject.GetComponent<Text>().text;

        EventSystem.gameObject.SetActive(false);

        SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
        this._playSceneName = sceneName;

    }

    public void SetScolllerActive()
    {
        CaseScrollerView.SetActive(false);
    }
}
