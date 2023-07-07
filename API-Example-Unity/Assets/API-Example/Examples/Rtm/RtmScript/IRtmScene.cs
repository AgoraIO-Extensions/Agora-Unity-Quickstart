#define AGORA_RTC
#define AGORA_RTM

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if AGORA_RTC
using Agora.Rtc;
#endif
using Agora.Rtm;

namespace io.agora.rtm.demo
{
    public class IRtmScene : MonoBehaviour
    {
#if AGORA_RTC
        public IRtcEngine RtcEngine = null;
#endif
        public IRtmClient RtmClient = null;
        public IStreamChannel StreamChannel = null;
        public IRtmLock RtmLock = null;
        public IRtmPresence RtmPresence = null;
        public IRtmStorage RtmStorage = null;
        public string JoinedChannelName = null;
        public uint JoinedUid = 0;
        public AppIdInput InfoInput;
        public MessageDisplay Display;

        public void Awake()
        {
            var comps = this.GetComponentsInChildren<IRtmComponet>();
            foreach (var rtmCom in comps)
            {
                rtmCom.Init(this);
            }
        }

        public void AddMessage(string text, Message.MessageType messageType)
        {
            this.Display.AddMessage(text, messageType);
        }

        public void PostSatusNotify()
        {
            var comps = this.GetComponentsInChildren<IRtmComponet>();
            foreach (var rtmCom in comps)
            {
                rtmCom.UpdateUI();
            }
        }

        public void OnDestroy()
        {
            if (RtmClient != null)
            {
                RtmClient.Dispose();
                RtmClient = null;
            }
            if (RtcEngine != null)
            {
                RtcEngine.Dispose();
            }
        }
    }
}
