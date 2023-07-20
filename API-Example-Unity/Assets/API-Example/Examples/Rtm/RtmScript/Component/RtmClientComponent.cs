#define AGORA_RTC
#define AGORA_RTM

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if AGORA_RTC
using Agora.Rtc;
#endif
using Agora.Rtm;

namespace io.agora.rtm.demo
{
    public class RtmClientComponent : IRtmComponet
    {
        public Text TitleText;
        public InputField AppIdInput;
        public InputField UsernameInput;
        public InputField presenceTimeoutInput;
        public Toggle UseStringUid;

        void Start()
        {
            this.AppIdInput.text = RtmScene.InfoInput.appID;
            this.TitleText.text = "Rtm Client not init";
            this.TitleText.color = Color.red;
            this.presenceTimeoutInput.text = "30";
        }

        public void OnInit()
        {
            string appId = AppIdInput.text;
            uint presenceTimeout = uint.Parse(presenceTimeoutInput.text);
            string username = UsernameInput.text;
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(appId))
            {
                RtmScene.AddMessage("We need a username and appId to init", Message.MessageType.Error);
                return;
            }

            RtmConfig config = new RtmConfig();
            config.appId = appId;
            config.userId = username;
            config.presenceTimeout = presenceTimeout;
            config.useStringUserId = this.UseStringUid.isOn;
            IRtmClient rtmClient = null;
            try
            {
                rtmClient = RtmClient.CreateAgoraRtmClient(config);
            }
            catch (RTMException e)
            {
                RtmScene.AddMessage("rtmClient.init error  ret:" + e.Status.ErrorCode, Message.MessageType.Error);
            }


            if (rtmClient != null)
            {
                //add observer
                rtmClient.OnMessageEvent += this.OnMessageEvent;
                rtmClient.OnPresenceEvent += this.OnPresenceEvent;
                rtmClient.OnTopicEvent += this.OnTopicEvent;
                rtmClient.OnStorageEvent += this.OnStorageEvent;
                rtmClient.OnLockEvent += this.OnLockEvent;
                rtmClient.OnConnectionStateChange += this.OnConnectionStateChange;
                rtmClient.OnTokenPrivilegeWillExpire += this.OnTokenPrivilegeWillExpire;


                //var ret = rtmClient.SetParameters("{\"rtm.link_address0\":[\"183.131.160.141\", 9130]}");
                //RtmScene.AddMessage("rtmClient.SetParameters + ret:" + ret, Message.MessageType.Info);
                //ret = rtmClient.SetParameters("{\"rtm.link_address1\":[\"183.131.160.142\", 9131]}");
                //RtmScene.AddMessage("rtmClient.SetParameters + ret:" + ret, Message.MessageType.Info);
                //ret = rtmClient.SetParameters("{\"rtm.link_encryption\": false}");
                //RtmScene.AddMessage("rtmClient.SetParameters + ret:" + ret, Message.MessageType.Info);
                ////ret = rtmClient.SetParameters("{\"rtm.ap_address\":[\"114.236.137.40\", 8443]}");
                //RtmScene.AddMessage("rtmClient.SetParameters + ret:" + ret, Message.MessageType.Info);


                RtmScene.AddMessage("rtmClient init success", Message.MessageType.Info);
                RtmScene.RtmClient = rtmClient;
                this.TitleText.text = "RtmClient alread init";
                this.TitleText.color = Color.green;
            }

        }

        public void OnMessageEvent(MessageEvent @event)
        {
            string str = string.Format("OnMessageEvent channelName:{0} channelTopic:{1} channelType:{2} publisher:{3} message:{4} customType:{5}",
              @event.channelName, @event.channelTopic, @event.channelType, @event.publisher, @event.message.GetData<string>(), @event.customType);
            RtmScene.AddMessage(str, Message.MessageType.Info);
        }

        public void OnPresenceEvent(PresenceEvent @event)
        {
            string str = string.Format("OnPresenceEvent: type:{0} channelType:{1} channelName:{2} publisher:{3}",
                @event.type, @event.channelType, @event.channelName, @event.publisher);
            RtmScene.AddMessage(str, Message.MessageType.Info);
        }

        public void OnStorageEvent(StorageEvent @event)
        {
            string str = string.Format("OnStorageEvent: channelType:{0} storageType:{1} eventType:{2} target:{3}",
                @event.channelType, @event.storageType, @event.eventType, @event.target);
            RtmScene.AddMessage(str, Message.MessageType.Info);
            if (@event.data != null)
            {
                DisplayRtmMetadata(ref @event.data);
            }
        }

        public void OnTopicEvent(TopicEvent @event)
        {
            string str = string.Format("OnTopicEvent: channelName:{0} publisher:{1}", @event.channelName, @event.publisher);
            RtmScene.AddMessage(str, Message.MessageType.Info);

            if (@event.topicInfoCount > 0)
            {
                for (ulong i = 0; i < @event.topicInfoCount; i++)
                {
                    var topicInfo = @event.topicInfos[i];
                    string str1 = string.Format("|--topicInfo {0}: topic:{1} publisherCount:{2}", i, topicInfo.topic, topicInfo.publisherCount);
                    RtmScene.AddMessage(str1, Message.MessageType.Info);
                    if (topicInfo.publisherCount > 0)
                    {
                        for (ulong j = 0; j < topicInfo.publisherCount; j++)
                        {
                            var publisher = topicInfo.publishers[j];
                            string str2 = string.Format("  |--publisher {0}: userId:{1} meta:{2}", j, publisher.publisherUserId, publisher.publisherMeta);
                            RtmScene.AddMessage(str2, Message.MessageType.Info);
                        }
                    }
                }
            }
        }

        public void OnLockEvent(LockEvent @event)
        {
            string info = string.Format("OnLockEvent channelType:{0}, eventType:{1}, channelName:{2}, count:{3}", @event.channelType, @event.eventType, @event.channelName, @event.count);
            RtmScene.AddMessage(info, Message.MessageType.Info);
            if (@event.count > 0)
            {
                for (int i = 0; i < @event.lockDetailList.Length; i++)
                {
                    var detail = @event.lockDetailList[i];
                    string info2 = string.Format("lockDetailList lockName:{0}, owner:{1}, ttl:{2}", detail.lockName, detail.owner, detail.ttl);
                    RtmScene.AddMessage(info2, Message.MessageType.Info);
                }
            }

        }

        public void OnConnectionStateChange(string channelName, RTM_CONNECTION_STATE state, RTM_CONNECTION_CHANGE_REASON reason)
        {
            string str1 = string.Format("OnConnectionStateChange channelName {0}: state:{1} reason:{2}", channelName, state, reason);
            RtmScene.AddMessage(str1, Message.MessageType.Info);
        }

        public void OnTokenPrivilegeWillExpire(string channelName)
        {
            string str1 = string.Format("OnTokenPrivilegeWillExpire channelName {0}", channelName);
            RtmScene.AddMessage(str1, Message.MessageType.Info);
        }

        private void DisplayRtmMetadata(ref RtmMetadata data)
        {
            RtmScene.AddMessage("RtmMetadata.majorRevision:" + data.majorRevision, Message.MessageType.Info);
            if (data.metadataItemsSize > 0)
            {
                foreach (var item in data.metadataItems)
                {
                    RtmScene.AddMessage(string.Format("key:{0},value:{1},authorUserId:{2},revision:{3},updateTs:{4}", item.key, item.value, item.authorUserId, item.revision, item.updateTs), Message.MessageType.Info);
                }
            }
        }
    }
}
