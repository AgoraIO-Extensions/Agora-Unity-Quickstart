using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Agora.Rtm;

namespace io.agora.rtm.demo
{
    public class RtmStreamChannelPublishTopicMessageComponent : IRtmComponet
    {
        public InputField TopicInput;
        public InputField MessageInput;
        public EnumDropDown TypeDropDown;
        public InputField SendTsInput;


        public void Start()
        {
            this.SendTsInput.text = "0";
        }

        public async void OnPublishTopicMessage()
        {
            if (this.RtmScene.StreamChannel == null)
            {
                this.RtmScene.AddMessage("StreamChannel is null", Message.MessageType.Error);
                return;
            }


            string topic = this.TopicInput.text;
            string message = this.MessageInput.text;
            if (topic == "" || message == "")
            {
                this.RtmScene.AddMessage("topic or message is empty", Message.MessageType.Error);
                return;
            }

            PublishOptions options = new PublishOptions();
            options.type = (RTM_MESSAGE_TYPE)this.TypeDropDown.GetSelectValue();
            options.sendTs = ulong.Parse(this.SendTsInput.text);

            var result = await this.RtmScene.StreamChannel.PublishTopicMessageAsync(topic, message, options);
            this.RtmScene.AddMessage("StreamChannel.PublishTopicMessage  ret:" + result.Status.ErrorCode, Message.MessageType.Info);
        }


    }
}
