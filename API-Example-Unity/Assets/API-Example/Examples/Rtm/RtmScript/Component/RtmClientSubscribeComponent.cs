using System.Collections;
using System.Collections.Generic;
using Agora.Rtc;
using Agora.Rtm;
using UnityEngine;
using UnityEngine.UI;

namespace io.agora.rtm.demo
{
    public class RtmClientSubscribeComponent : IRtmComponet
    {
        public InputField ChannelNameInput;
        public Toggle WithMessageToggle;
        public Toggle WithMetadataToggle;
        public Toggle WithPersenceToggle;
        public Toggle WithLockToggle;

        public List<string> ChannelNameList = new List<string>();

        public async void onSubscribe()
        {
            if (RtmScene.RtmClient == null)
            {
                RtmScene.AddMessage("RtmClient not init!!!", Message.MessageType.Error);
                return;
            }

            string channelName = this.ChannelNameInput.text;
            if (channelName == "")
            {
                RtmScene.AddMessage("channelName is empty", Message.MessageType.Error);
                return;
            }

            SubscribeOptions subscribeOptions = new SubscribeOptions()
            {
                withMessage = WithMessageToggle.isOn,
                withMetadata = WithMessageToggle.isOn,
                withPresence = WithPersenceToggle.isOn,
                withLock = WithLockToggle.isOn
            };

            var result = await RtmScene.RtmClient.SubscribeAsync(channelName, subscribeOptions);

            if (result.Status.Error)
            {
                RtmScene.AddMessage(string.Format("rtmClient.Subscribe Status.ErrorCode:{0} ", result.Status.ErrorCode), Message.MessageType.Info);
            }
            else
            {
                string info = string.Format("rtmClient.Subscribe Response , channelName:{0}, errorCode:{1}", result.Response.ChannelName, result.Response.ErrorCode);
                RtmScene.AddMessage(info, Message.MessageType.Info);
                ChannelNameList.Add(channelName);
            }
        }


        public async void OnUnsubscribe()
        {
            if (RtmScene.RtmClient == null)
            {
                RtmScene.AddMessage("RtmClient not init!!!", Message.MessageType.Error);
                return;
            }

            string channelName = this.ChannelNameInput.text;
            var result = await RtmScene.RtmClient.UnsubscribeAsync(channelName);

            RtmScene.AddMessage("rtmClient.Unsubscribe  ret:" + result.Status.ErrorCode, Message.MessageType.Info);
            if (!result.Status.Error)
            {
                ChannelNameList.Remove(channelName);
            }
        }

        public void OnMark()
        {
            string show = "alread subscribe channl. size:" + ChannelNameList.Count + "\n";
            foreach (string channelName in ChannelNameList)
            {
                show += ("    " + channelName + "\n");
            }
            RtmScene.AddMessage(show, Message.MessageType.Info);
        }
    }
}
