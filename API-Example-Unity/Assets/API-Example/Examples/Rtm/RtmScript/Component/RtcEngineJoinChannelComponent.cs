using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Agora.Rtc;
using Agora.Rtm;

namespace io.agora.rtm.demo
{
    public class RtcEngineJoinChannelComponent : IRtmComponet
    {
        public Text TitleText;
        public InputField ChannelNameInput;
        public InputField TokenInput;
        public InputField UidInput;

        private void Start()
        {
            this.TitleText.text = "RtcEngine not join channel yet";
            this.TitleText.color = Color.red;

            this.ChannelNameInput.text = this.RtmScene.InfoInput.channelName;
            this.TokenInput.text = this.RtmScene.InfoInput.token;
            this.UidInput.text = "0";
        }


        public void OnJoinChannel()
        {
            if (this.RtmScene.RtcEngine == null)
            {
                this.RtmScene.AddMessage("RtcEngine is null", Message.MessageType.Error);
                return;
            }

            if (this.ChannelNameInput.text == "")
            {
                this.RtmScene.AddMessage("RtcEngine channel name is empty", Message.MessageType.Error);
                return;
            }

            uint uid = uint.Parse(this.UidInput.text);
            int ret = this.RtmScene.RtcEngine.JoinChannel(this.TokenInput.text, this.ChannelNameInput.text,"", uid);
            this.RtmScene.AddMessage("RtcEngine JoinChannel : " + ret, ret == 0 ? Message.MessageType.Info : Message.MessageType.Error);
        }

        public void OnLeaveChannel()
        {
            if (this.RtmScene.RtcEngine == null)
            {
                this.RtmScene.AddMessage("RtcEngine is null", Message.MessageType.Error);
                return;
            }

            if (this.RtmScene.JoinedChannelName == null)
            {
                this.RtmScene.AddMessage("RtcEngine not join channel yet", Message.MessageType.Error);
                return;
            }


            int ret = this.RtmScene.RtcEngine.LeaveChannel();
            this.RtmScene.AddMessage("RtcEngine LeaveChannel : " + ret, ret == 0 ? Message.MessageType.Info : Message.MessageType.Error);
        }

        public override void UpdateUI()
        {
            if (this.RtmScene.JoinedChannelName != null)
            {

                this.TitleText.text = "RtcEngine already join channel: " + this.RtmScene.JoinedChannelName + "," + this.RtmScene.JoinedUid;
                this.TitleText.color = Color.green;
            }
            else
            {
                this.TitleText.text = "RtcEngine not join channel yet";
                this.TitleText.color = Color.red;
            }
        }

    }
}
