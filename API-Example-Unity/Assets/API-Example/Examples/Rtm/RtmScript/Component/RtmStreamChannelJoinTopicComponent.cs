#define AGORA_RTC
#define AGORA_RTM

using System.Collections;
using System.Collections.Generic;
using Agora.Rtc;
using Agora.Rtm;
using UnityEngine;
using UnityEngine.UI;

namespace io.agora.rtm.demo
{
    public class RtmStreamChannelJoinTopicComponent : IRtmComponet
    {
        public InputField TopicInput;
        public EnumDropDown QosDropDown;
        public EnumDropDown PriorityDropDown;
        public InputField MetaInput;
        public Toggle SyncWithMediaToggle;
        public List<string> TopicList = new List<string>();


        public void Start()
        {
            this.QosDropDown.Init<RTM_MESSAGE_QOS>();
            this.PriorityDropDown.Init<RTM_MESSAGE_PRIORITY>();
        }

        public async void OnJoinTopic()
        {
            if (this.RtmScene.StreamChannel == null)
            {
                this.RtmScene.AddMessage("StreamChannel not created!", Message.MessageType.Error);
                return;
            }
            if (this.TopicInput.text == "")
            {
                this.RtmScene.AddMessage("Topic name not input!", Message.MessageType.Error);
                return;
            }

            JoinTopicOptions options = new JoinTopicOptions()
            {
                qos = (RTM_MESSAGE_QOS)this.QosDropDown.GetSelectValue(),
                priority = (RTM_MESSAGE_PRIORITY)this.PriorityDropDown.GetSelectValue(),
                meta = this.MetaInput.text,
                syncWithMedia = this.SyncWithMediaToggle.isOn
            };

            var result = await this.RtmScene.StreamChannel.JoinTopicAsync(this.TopicInput.text, options);
            if (result.Status.Error)
            {
                this.RtmScene.AddMessage(string.Format("StreamChannel.JoinTopic Status.ErrorCode:{0} ", result.Status.ErrorCode), Message.MessageType.Error);
            }
            else
            {
                string str = string.Format("StreamChannel.JoinTopic Response: channelName:{0} userId:{1} topic:{2} meta:{3}",
                  result.Response.ChannelName, result.Response.UserId, result.Response.Topic, result.Response.Meta);
                this.RtmScene.AddMessage(str, Message.MessageType.Info);
                this.TopicList.Add(this.TopicInput.text);
            }

        }

        public async void OnLeaveTopic()
        {
            if (this.RtmScene.StreamChannel == null)
            {
                this.RtmScene.AddMessage("StreamChannel not created!", Message.MessageType.Error);
                return;
            }
            if (this.TopicInput.text == "")
            {
                this.RtmScene.AddMessage("Topic name not input!", Message.MessageType.Error);
                return;
            }

            var result = await this.RtmScene.StreamChannel.LeaveTopicAsync(this.TopicInput.text);

            if (result.Status.Error)
            {
                this.RtmScene.AddMessage(string.Format("StreamChannel.LeaveTopic Status.ErrorCode:{0} ", result.Status.ErrorCode), Message.MessageType.Info);
            }
            else
            {
                string str = string.Format("StreamChannel.LeaveTopic Response: channelName:{0} userId:{1} topic:{2} meta:{3}",
                  result.Response.ChannelName, result.Response.UserId, result.Response.Topic, result.Response.Meta);
                this.RtmScene.AddMessage(str, Message.MessageType.Info);
                this.TopicList.Remove(this.TopicInput.text);
            }
        }


        public void OnMark()
        {
            string show = "already join topic. size:" + TopicList.Count + "\n";
            foreach (string topiclName in TopicList)
            {
                show += ("    " + topiclName + "\n");
            }
            RtmScene.AddMessage(show, Message.MessageType.Info);

        }

    }
}
