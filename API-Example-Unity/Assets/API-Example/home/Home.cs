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
    public GameObject casePanel;
    public GameObject caseScoller;
    private string PlaySceneName = "";
    private string[] PlaySceneNameList = {"BasicVideoCallScene", "BasicAudioCallScene", "AudioMixingScene", "ScreenShareScene", 
                                            "DeviceManagerScene", "ScreenShareWhileVideoCallScene", "SpatialAudioWithMediaPlayerScene"};
    
    private void Awake()
    {
#if (UNITY_2018_3_OR_NEWER && UNITY_ANDROID)
        PermissionHelper.RequestMicrophontPermission();
        PermissionHelper.RequestCameraPermission();
#endif

        GameObject content = GameObject.Find("Content");
        var contentRectTrans = content.GetComponent<RectTransform>();
        //contentRectTrans.SetSizeWithCurrentAnchors();
        for(int i = 0; i < PlaySceneNameList.Length; i++)
        {
            var go = Instantiate(casePanel, content.transform);
            var name = go.transform.Find("Text").gameObject.GetComponent<Text>();
            name.text = PlaySceneNameList[i];
            var button = go.transform.Find("Button").gameObject.GetComponent<Button>();
            button.onClick.AddListener(OnJoinSceneClicked);
            button.onClick.AddListener(SetScolllerActive);

            //var rectTrans = go.GetComponent<RectTransform>();
            //rectTrans.anchoredPosition = new Vector2(0, i * (-100));
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
        IAgoraRtcEngine _mRtcEngine = AgoraRtcEngine.Get();
        if (_mRtcEngine != null)
        {
            _mRtcEngine.Dispose(true);
        }
    }

    public void OnLeaveButtonClicked()
    {
        StartCoroutine(UnloadSceneAsync());
        caseScoller.SetActive(true);
    }

    public IEnumerator UnloadSceneAsync()
    {
        if (this.PlaySceneName!="")
        {
            AsyncOperation async = SceneManager.UnloadSceneAsync(PlaySceneName);
            yield return async;
        }
    }

    public void OnJoinSceneClicked()
    {
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
