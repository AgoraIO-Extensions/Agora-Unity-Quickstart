using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Agora.Rtc;
using Agora.Rtm;

namespace io.agora.rtm.demo
{
    public class RtmClientPublishComponent : IRtmComponet
    {
        public InputField ChannelNameInput;
        public InputField MessageInput;
        public InputField SendTsInput;
        public EnumDropDown enumDropDown;

        public void Start()
        {
            this.enumDropDown.Init<RTM_MESSAGE_TYPE>();
        }

        public async void OnPublish()
        {
            if (RtmScene.RtmClient == null)
            {
                RtmScene.AddMessage("RtmClient not init!!!", Message.MessageType.Error);
                return;
            }

            string message = MessageInput.text;
            if (message == "")
            {
                RtmScene.AddMessage("Message is empty", Message.MessageType.Error);
                return;
            }

            string channelName = this.ChannelNameInput.text;

            PublishOptions options = new PublishOptions();
            if (this.SendTsInput.text != "")
            {
                options.sendTs = ulong.Parse(this.SendTsInput.text);
            }
            else
            {
                options.sendTs = 0;
            }
            options.type = (RTM_MESSAGE_TYPE)this.enumDropDown.GetSelectValue();


            var result = await RtmScene.RtmClient.PublishAsync(channelName, message, options);

            if (result.Status.Error)
            {
                RtmScene.AddMessage(string.Format("rtmClient.Publish Status.ErrorCode:{0} ", result.Status.ErrorCode), Message.MessageType.Error);
            }
            else
            {
                string info = string.Format("rtmClient.Publish Response , errorCode:{0}", result.Response.ErrorCode);
                RtmScene.AddMessage(info, Message.MessageType.Info);
            }

        }
    }
}
