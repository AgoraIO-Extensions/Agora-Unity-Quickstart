using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Agora.Rtc;
using Agora.Rtm;

namespace io.agora.rtm.demo
{
    public class IRtmScene : MonoBehaviour
    {
        public IRtcEngine RtcEngine = null;
        public IRtmClient RtmClient = null;
        public IStreamChannel StreamChannel = null;
        public IRtmLock RtmLock = null;
        public IRtmPresence RtmPresence = null;
        public IRtmStorage RtmStorage = null;
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
    }
}
