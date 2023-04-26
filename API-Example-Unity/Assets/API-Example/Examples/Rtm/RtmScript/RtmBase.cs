using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Agora.Rtc;
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

    private IRtcEngine _rtcEngine;
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
