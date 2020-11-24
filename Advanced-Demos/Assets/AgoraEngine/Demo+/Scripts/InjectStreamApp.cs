using UnityEngine;
using UnityEngine.UI;
using agora_gaming_rtc;

public class InjectStreamApp : PlayerViewControllerBase
{
    bool _injecting = false;
    public string InjectURL { get; set; }

    protected override void PrepareToJoin()
    {
        base.PrepareToJoin();
        mRtcEngine.SetChannelProfile(CHANNEL_PROFILE.CHANNEL_PROFILE_LIVE_BROADCASTING);
        mRtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);

        mRtcEngine.OnStreamInjectedStatus = OnStreamInjectedStatus;
    }

    protected override void SetupUI()
    {
        base.SetupUI();
        Button ibtn = GameObject.Find("InjectButton").GetComponent<Button>();
        ibtn.onClick.AddListener(() =>
        {
            HandleInjectButton(ibtn);

        });


        TextAsset txt = (TextAsset)Resources.Load("injectURL", typeof(TextAsset));
        InjectURL = txt.text.Trim();
        Debug.Log("InjectURL = " + InjectURL);

        txt = (TextAsset)Resources.Load("wiki_URL", typeof(TextAsset));
        string wikiURL = txt.text.Trim();

        Button ibtn2 = GameObject.Find("HelpButton").GetComponent<Button>();
        ibtn2.onClick.AddListener(() =>
        {
            Application.OpenURL(wikiURL);
        });
    }

    protected override void OnUserOffline(uint uid, USER_OFFLINE_REASON reason)
    {
        base.OnUserOffline(uid, reason);
        GameObject go = GameObject.Find(uid.ToString());
        if (!ReferenceEquals(go, null))
        {
            Object.Destroy(go);
        }
    }

    void HandleInjectButton(Button ibtn)
    {
        if (_injecting)
        {
            StopStream(InjectURL);
        }
        else
        {
            InjectStream(InjectURL);
        }
        ibtn.GetComponentInChildren<Text>().text = _injecting ? "Stop Injectin" : "Inject Stream";
    }

    void InjectStream(string url)
    {
        InjectStreamConfig injectStreamConfig = new InjectStreamConfig();
        injectStreamConfig.width = 0;
        injectStreamConfig.height = 0;
        injectStreamConfig.videoGop = 30;
        injectStreamConfig.videoFramerate = 15;
        injectStreamConfig.videoBitrate = 400;
        injectStreamConfig.audioSampleRate = AUDIO_SAMPLE_RATE_TYPE.AUDIO_SAMPLE_RATE_44100;
        injectStreamConfig.audioChannels = 1;
        injectStreamConfig.audioBitrate = 48000;

        // Inject an online media stream.
        int ret = mRtcEngine.AddInjectStreamUrl(url, injectStreamConfig);

        Debug.LogWarning("Injecting RTMP ret = :" + ret);
        _injecting = true;
    }

    void StopStream(string url)
    {
        // Remove an online media stream.
        mRtcEngine.RemoveInjectStreamUrl(url);
        Debug.Log("Stopping RTMP URL:" + url);
        _injecting = false;
    }

    void OnStreamInjectedStatus(string url, uint userId, int status)
    {
        Debug.LogFormat("OnStreamInjectedStatus({0}) user:{1}, status:{2}", url, userId, status);
    }

}
