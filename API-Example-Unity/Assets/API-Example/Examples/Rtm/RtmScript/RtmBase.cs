using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if AGORA_RTC
using Agora.Rtc;
#endif
using Agora.Rtm;
using UnityEngine.Serialization;

public class RtmBase : MonoBehaviour
{
    [FormerlySerializedAs("appIdInput")]
    [SerializeField]
    private AppIdInput _appIdInput;

    public Text RtcEngineText;
    public Text RtmClientText;

    public InputField AppIdInputBox;
    public InputField TokenInputBox;

#if AGORA_RTC
    RtcEngine _rtcEngine;
#endif
    private IRtmClient _rtmClient;


    // Start is called before the first frame update
    void Start()
    {

        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
