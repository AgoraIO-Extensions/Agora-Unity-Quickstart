#define AGORA_RTC
#define AGORA_RTM
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

#if AGORA_RTC
            this.TitleText.text = "RtcEngine not init";
            this.TitleText.color = Color.red;
#else
            this.TitleText.text = "RtcEngine not include sdk";
            this.TitleText.color = Color.yellow;
#endif
        }

        public void OnInit()
        {
#if AGORA_RTC
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
            RtmScene.AddMessage("RtcEngine Init :" + nRet, nRet == 0 ? Message.MessageType.Info : Message.MessageType.Error);
            rtcEngine.InitEventHandler(handler);

            RtmScene.RtcEngine = rtcEngine;
            this.TitleText.text = "RtcEngine already init";
            this.TitleText.color = Color.green;
#endif
        }
    }

#if AGORA_RTC
    internal class UserEventHandler : IRtcEngineEventHandler
    {
        public RtcEngineComponent EngineComponent;
        public UserEventHandler(RtcEngineComponent engineComponent)
        {
            this.EngineComponent = engineComponent;
        }

        public override void OnJoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            this.EngineComponent.RtmScene.AddMessage("RtcEngine join channel Sucess", Message.MessageType.Info);
            this.EngineComponent.RtmScene.JoinedChannelName = connection.channelId;
            this.EngineComponent.RtmScene.JoinedUid = connection.localUid;
            this.EngineComponent.RtmScene.PostSatusNotify();
        }

        public override void OnLeaveChannel(RtcConnection connection, RtcStats stats)
        {
            this.EngineComponent.RtmScene.AddMessage("RtcEngine leave channel Sucess", Message.MessageType.Info);
            this.EngineComponent.RtmScene.JoinedChannelName = null;
            this.EngineComponent.RtmScene.JoinedUid = 0;
            this.EngineComponent.RtmScene.PostSatusNotify();
        }

        public override void OnError(int err, string msg)
        {
            string show = string.Format("RtcEngine OnError err:{0} msg:{1}", err, msg);
            this.EngineComponent.RtmScene.AddMessage(show, Message.MessageType.Error);
        }
    }
#endif

}
