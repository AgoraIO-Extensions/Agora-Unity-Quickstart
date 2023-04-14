using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Agora.Rtc;
using Agora.Rtm;

namespace io.agora.rtm.demo
{
    public class RtcEngineComponent : IRtmComponet
    {
        public Text TitleText;
        public InputField AppIdInput;
        public InputField TokenInput;

        private void Start()
        {
            this.AppIdInput.text = RtmScene.InfoInput.appID;
            this.TokenInput.text = RtmScene.InfoInput.token;

            this.TitleText.text = "RtcEngine not init";
            this.TitleText.color = Color.red;
        }

        public void OnInit()
        {
            var appId = this.AppIdInput.text;
            if (appId == "")
            {
                RtmScene.AddMessage("Appid is empty!!!", Message.MessageType.Error);
                return;
            }

            var token = this.TokenInput.text;
            IRtcEngineEx rtcEngine = RtcEngine.CreateAgoraRtcEngineEx();

            UserEventHandler handler = new UserEventHandler(this);
            RtcEngineContext context = new RtcEngineContext(appId, 0,
                CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING,
                AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_GAME_STREAMING);
            int nRet = rtcEngine.Initialize(context);
            RtmScene.AddMessage("RtcEngine Init :" + nRet, Message.MessageType.Info);
            rtcEngine.InitEventHandler(handler);

            RtmScene.RtcEngine = rtcEngine;
            this.TitleText.text = "RtcEngine already init";
            this.TitleText.color = Color.green;
        }
    }


    internal class UserEventHandler : IRtcEngineEventHandler
    {
        public RtcEngineComponent EngineComponent;
        public UserEventHandler(RtcEngineComponent engineComponent)
        {
            this.EngineComponent = engineComponent;
        }
    }

}
