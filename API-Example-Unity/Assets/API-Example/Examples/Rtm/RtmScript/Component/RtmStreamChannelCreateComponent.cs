#define AGORA_RTC
#define AGORA_RTM

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Agora.Rtm;
using UnityEngine.UI;

namespace io.agora.rtm.demo
{
    public class RtmStreamChannelCreateComponent : IRtmComponet
    {
        public Text TitleText;
        public InputField ChannelNameInput;
        public InputField TokenInput;
        public Toggle WithMetadataToggle;
        public Toggle WithPresenceToggle;
        public Toggle WithLockToggle;
        public Toggle BeQuietToggle;

        void Start()
        {
            this.TitleText.text = "IStreamChannel not created";
            this.TitleText.color = Color.red;
        }


        public void OnCreateFromRtmClient()
        {
            if (this.RtmScene.RtmClient == null)
            {
                this.RtmScene.AddMessage("Rtm Client is null", Message.MessageType.Error);
                return;
            }

            if (this.ChannelNameInput.text == "")
            {
                this.RtmScene.AddMessage("Channel name is empty!", Message.MessageType.Error);
            }

            int errorCode = 0;
            this.RtmScene.StreamChannel = this.RtmScene.RtmClient.CreateStreamChannel(this.ChannelNameInput.text, ref errorCode);
            if (this.RtmScene.StreamChannel != null && errorCode == 0)
            {
                this.TitleText.text = "StreamChannel is created from rtmClient";
                this.TitleText.color = Color.green;
            }
            else
            {
                this.RtmScene.AddMessage("create rtm stream channel failed: " + errorCode, Message.MessageType.Error);
            }
        }

        public void OnGetFromRtcEngine()
        {
#if AGORA_RTC
            //if (this.RtmScene.RtcEngine == null)
            //{
            //    this.RtmScene.AddMessage("Rtc Engine is null", Message.MessageType.Error);
            //    return;
            //}

            //if (this.RtmScene.RtmClient == null)
            //{
            //    this.RtmScene.AddMessage("Rtm Client is null", Message.MessageType.Error);
            //    return;
            //}

            //if (this.ChannelNameInput.text == "")
            //{
            //    this.RtmScene.AddMessage("Channel name is empty!", Message.MessageType.Error);
            //}

            //this.RtmScene.StreamChannel = this.RtmScene.RtcEngine.GetStreamChannel(this.ChannelNameInput.text);
            //if (this.RtmScene.StreamChannel != null)
            //{
            //    this.TitleText.text = "StreamChannel is created from rtmClient";
            //    this.TitleText.color = Color.green;
            //}
            //else
            //{
            //    this.RtmScene.AddMessage("create rtm stream channel failed", Message.MessageType.Error);
            //}

            this.RtmScene.AddMessage("rtc engine is not contains GetStreamChannel", Message.MessageType.Error);
#else
            this.RtmScene.AddMessage("rtc engine is not include in sdk", Message.MessageType.Error);
#endif

        }


        public async void OnJoin()
        {
            if (this.RtmScene.StreamChannel == null)
            {
                this.RtmScene.AddMessage("Rtm StreamChannel is null", Message.MessageType.Error);
                return;
            }

            JoinChannelOptions options = new JoinChannelOptions();
            options.token = this.TokenInput.text;
            options.withMetadata = this.WithMetadataToggle.isOn;
            options.withPresence = this.WithPresenceToggle.isOn;
            options.withLock = this.WithLockToggle.isOn;
            options.beQuiet = this.BeQuietToggle.isOn;

            var result = await this.RtmScene.StreamChannel.JoinAsync(options);
            if (result.Status.Error)
            {
                this.RtmScene.AddMessage(string.Format("Join Status.ErrorCode:{0} ", result.Status.ErrorCode), Message.MessageType.Error);
            }
            else
            {
                string str = string.Format("Join Response: channelName:{0} userId:{1}",
                    result.Response.ChannelName, result.Response.UserId);
                this.RtmScene.AddMessage(str, Message.MessageType.Info);
                this.TitleText.text = "Steam channel already joined";
                this.TitleText.color = Color.green;
            }
        }

        public async void OnLeave()
        {
            if (this.RtmScene.StreamChannel == null)
            {
                this.RtmScene.AddMessage("Rtm StreamChannel is null", Message.MessageType.Error);
                return;
            }

            var result = await this.RtmScene.StreamChannel.LeaveAsync();

            if (result.Status.Error)
            {
                this.RtmScene.AddMessage(string.Format("StreamChannel.Leave Status.ErrorCode:{0} ", result.Status.ErrorCode), Message.MessageType.Error);
            }
            else
            {
                string str = string.Format("StreamChannel.Leave Response: channelName:{0} userId:{1}",
                    result.Response.ChannelName, result.Response.UserId);
                this.RtmScene.AddMessage(str, Message.MessageType.Info);
                this.TitleText.text = "Steam channel already leave";
                this.TitleText.color = Color.red;
            }

        }

        public void OnReleaseStreamChannel()
        {
            if (this.RtmScene.StreamChannel == null)
            {
                this.RtmScene.AddMessage("rtm stream channel is empty", Message.MessageType.Error);
                return;
            }

            this.RtmScene.StreamChannel.Dispose();
            this.RtmScene.StreamChannel = null;
            this.TitleText.text = "rtm stream channel released";
            this.TitleText.color = Color.red;
        }

    }

}