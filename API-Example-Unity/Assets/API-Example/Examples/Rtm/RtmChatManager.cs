using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Agora.Rtm;
using Agora.Rtc;
using io.agora.rtm.demo;
using System;

namespace io.agora.rtm.demo
{
    public class RtmChatManager : MonoBehaviour
    {

        [Header("Agora Properties")]
        [SerializeField]
        private string appId = "";
        [SerializeField]
        private string token = "";

        private string topic = "";
        private string subtopic = "";

        [Header("Application Properties")]

        [SerializeField] InputField userNameInput, channelNameInput;

        [SerializeField] InputField AppIdInputBox, TokenInputBox;

        [SerializeField] InputField TopicNameBox;
        [SerializeField] InputField TopicMessageBox;
        [SerializeField] InputField TopicSubscribedBox;
        [SerializeField] InputField UserAddBox;
        [SerializeField] Text appIdDisplayText;
        [SerializeField] Text tokenDisplayText;

        [SerializeField] MessageDisplay messageDisplay;

        private IRtcEngine rtcEngine;
        private IRtmClient rtmClient;
        private IStreamChannel streamChannel;

        private List<string> userList = new List<string>();


        string _userName = "";
        string UserName
        {
            get { return _userName; }
            set
            {
                _userName = value;
                PlayerPrefs.SetString("RTM_USER", _userName);
                PlayerPrefs.Save();
            }
        }

        string _channelName = "";
        string ChannelName
        {
            get { return _channelName; }
            set
            {
                _channelName = value;
                PlayerPrefs.SetString("RTM_CHANNEL", _channelName);
                PlayerPrefs.Save();
            }
        }

        public void Awake()
        {
            userNameInput.text = PlayerPrefs.GetString("RTM_USER", "");
            channelNameInput.text = PlayerPrefs.GetString("RTM_CHANNEL", "");
        }

        public void InitRtcEngine()
        {
            appId = appId == "" ? AppIdInputBox.text : appId;
            token = token == "" ? TokenInputBox.text : token;

            rtcEngine = RtcEngine.CreateAgoraRtcEngine();
            if (rtcEngine != null)
            {
                int init = rtcEngine.Initialize(new RtcEngineContext(appId, 0,
                                CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING,
                                AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT, Agora.Rtc.AREA_CODE.AREA_CODE_CN));
                rtcEngine.SetParameters("{\"rtc.vos_list\":[\"114.236.138.120:4052\"]}");
                messageDisplay.AddMessage("rtcEngine.Initialize + ret:" + init, Message.MessageType.Info);
            }
            ShowDisplayTexts();
        }

        private void OnDestroy()
        {
            if (streamChannel != null)
            {
                UInt64 requestId = 0;
                var ret = streamChannel.Leave(ref requestId);
                streamChannel.Dispose();
                messageDisplay.AddMessage("StreamChannel.Leave + ret:" + ret, Message.MessageType.Info);
            }
            if (rtmClient != null)
            {
                rtmClient.Dispose();
                rtmClient = null;
            }
            if (rtcEngine != null)
            {
                rtcEngine.Dispose();
            }
        }

        #region Button Events
        public void Initialize()
        {
            UserName = userNameInput.text;

            if (string.IsNullOrEmpty(UserName) || string.IsNullOrEmpty(appId))
            {
                Debug.LogError("We need a username and appId to login");
                return;
            }
            rtmClient = RtmClient.CreateAgoraRtmClient();

            RtmConfig config = new RtmConfig();
            config.appId = appId;
            RtmEventHandler rtmEventHandler = new RtmEventHandler();

            config.eventHandler = rtmEventHandler;

            config.userId = UserName;
            if (rtmClient != null)
            {
                var ret = rtmClient.Initialize(config);
                messageDisplay.AddMessage("rtmClient.Initialize + ret:" + ret, Message.MessageType.Info);
            }
            rtmEventHandler.messageDisplay = messageDisplay;


        }

        public void JoinChannel()
        {
            ChannelName = channelNameInput.text;
            if (rtmClient != null)
            {
                if (streamChannel == null)
                {
                    streamChannel = rtmClient.CreateStreamChannel(ChannelName);
                }

                JoinChannelOptions options = new JoinChannelOptions();
                options.token = token;
                if (streamChannel != null)
                {
                    UInt64 requestId = 0;
                    int ret = streamChannel.Join(options, ref requestId);
                    messageDisplay.AddMessage(string.Format("StreamChannel.JoinChannel ret:{0} requestId:{1}", ret, requestId), Message.MessageType.Info);
                }
                else
                {
                    messageDisplay.AddMessage("StreamChannel is invalid: ChannelName is invalid", Message.MessageType.Error);
                }
            }
        }
        public void ChannelLeave()
        {
            if (streamChannel != null)
            {
                UInt64 requestId = 0;
                int ret = streamChannel.Leave(ref requestId);

                messageDisplay.AddMessage(string.Format("StreamChannel.ChannelLeave ret:{0} requestId:{1}", ret, requestId), Message.MessageType.Info);
            }
        }

        public void ChannelDispose()
        {
            if (rtmClient != null && streamChannel != null)
            {
                streamChannel.Dispose();
            }

        }

        public void RtmDispose()
        {
            if (rtmClient != null)
            {
                ChannelDispose();
                rtmClient.Dispose();
                rtmClient = null;
            }
        }

        public void JoinTopic()
        {
            topic = TopicNameBox.text;

            JoinTopicOptions joinTopicOptions = new JoinTopicOptions();

            if (streamChannel != null)
            {
                UInt64 requestId = 0;
                int ret = streamChannel.JoinTopic(topic, joinTopicOptions, ref requestId);

                messageDisplay.AddMessage("StreamChannel.GetChannelName ret:" + streamChannel.GetChannelName(), Message.MessageType.Info);

                messageDisplay.AddMessage(string.Format("StreamChannel.JoinTopic ret:{0} requestId:{1}", ret, requestId), Message.MessageType.Info);
            }

        }

        public void LeaveTopic()
        {
            if (streamChannel != null)
            {
                UInt64 requestId = 0;
                int ret = streamChannel.LeaveTopic(topic, ref requestId);

                messageDisplay.AddMessage(string.Format("StreamChannel.LeaveTopic ret:{0} requestId:{1}", ret, requestId), Message.MessageType.Info);
            }
        }


        public void TopicSubscribed()
        {
            subtopic = TopicSubscribedBox.text;

            TopicOptions topicOptions = new TopicOptions();

            if (userList != null && userList.Count > 0)
            {
                topicOptions.users = userList.ToArray();
                topicOptions.userCount = (uint)userList.Count;
            }
            UInt64 requestId = 0;
            int ret = streamChannel.SubscribeTopic(subtopic, topicOptions, ref requestId);

            messageDisplay.AddMessage(string.Format("StreamChannel.SubscribeTopic ret:{0} requestId:{1}", ret, requestId), Message.MessageType.Info);

            userList.Clear();
        }

        public void TopicUnSubscribed()
        {
            TopicOptions topicOptions = new TopicOptions();

            if (userList != null && userList.Count > 0)
            {
                topicOptions.users = userList.ToArray();
                topicOptions.userCount = (uint)userList.Count;
            }

            int ret = streamChannel.UnsubscribeTopic(subtopic, topicOptions);

            messageDisplay.AddMessage("StreamChannel.UnsubscribeTopic ret:" + ret, Message.MessageType.Info);
        }

        public void GetSubscribedUserList()
        {
            if (streamChannel != null)
            {
                UserList userList = new UserList();

                int ret = streamChannel.GetSubscribedUserList(subtopic, ref userList);

                messageDisplay.AddMessage("StreamChannel.GetSubscribedTopic ret:" + ret + " userListCount : " + userList.userCount, Message.MessageType.Info);

                for (int i = 0; i < userList.userCount; i++)
                {
                    messageDisplay.AddMessage("StreamChannel.GetSubscribedTopic userIndex : " + i + " userListName : " + userList.users[i], Message.MessageType.Info);
                }
            }

        }

        public void SendTopicMessage()
        {
            byte[] message = System.Text.Encoding.Default.GetBytes(TopicMessageBox.text);
            if (streamChannel != null)
            {
                PublishOptions publishOptions = new PublishOptions();
                publishOptions.type = RTM_MESSAGE_TYPE.RTM_MESSAGE_TYPE_STRING;
                publishOptions.sendTs = 0;

                int ret = streamChannel.PublishTopicMessage(topic, message, (ulong)message.Length, publishOptions);

                messageDisplay.AddMessage("StreamChannel.PublishTopicMessage  ret:" + ret, Message.MessageType.Info);
            }
        }

        //public void SendTopicMessageStr()
        //{
        //    if (streamChannel != null)
        //    {
        //        int ret = streamChannel.PublishTopicMessage(topic, TopicMessageBox.text);

        //        messageDisplay.AddMessage("StreamChannel.PublishTopicMessage  ret:" + ret, Message.MessageType.Info);
        //    }
        //}

        public void AddUser()
        {
            if (!userList.Contains(UserAddBox.text))
            {
                userList.Add(UserAddBox.text);

                messageDisplay.AddMessage("AddUser " + UserAddBox.text + "to add/remove list", Message.MessageType.Info);
            }
            else
            {
                messageDisplay.AddMessage("This user has already been added", Message.MessageType.Info);
            }
        }
        #endregion

        private bool ShowDisplayTexts()
        {
            int showLength = 6;
            if (string.IsNullOrEmpty(appId) || appId.Length < showLength)
            {
                Debug.LogError("App ID is not set, please set it in " + gameObject.name);
                appIdDisplayText.text = "APP ID NOT SET";
                appIdDisplayText.color = Color.red;
                return false;
            }
            else
            {
                appIdDisplayText.text = "appid = ********" + appId.Substring(appId.Length - showLength, showLength);
            }

            if (string.IsNullOrEmpty(token) || token.Length < showLength)
            {
                tokenDisplayText.text = "token = null";
            }
            else
            {
                tokenDisplayText.text = "token = ********" + token.Substring(token.Length - showLength, showLength);

            }
            return true;
        }
    }

}


internal class RtmEventHandler : IRtmEventHandler
{
    public io.agora.rtm.demo.MessageDisplay messageDisplay;

    public override void OnMessageEvent(MessageEvent @event)
    {
        messageDisplay.AddMessage("OnMessageEvent channelName : " + @event.channelName + " channelTopic :" + @event.channelTopic + " channelType : " + @event.channelType + " publisher : " + @event.publisher + " message : " + @event.message, Message.MessageType.TopicMessage);
    }

    public override void OnPresenceEvent(PresenceEvent @event)
    {
        string str = string.Format("OnPresenceEvent: type:{0} channelType:{1} channelName:{2} publisher:{3}", @event.type, @event.channelType, @event.channelName, @event.publisher);

        messageDisplay.AddMessage(str, Message.MessageType.Info);
    }

    public override void OnJoinResult(UInt64 requestId, string channelName, string userId, RTM_CHANNEL_ERROR_CODE errorCode)
    {
        string str = string.Format("OnJoinResult: requestId:{0} channelName:{1} userId:{2} errorCode:{3}", requestId, channelName, userId, errorCode);

        messageDisplay.AddMessage(str, Message.MessageType.Info);
    }

    public override void OnLeaveResult(UInt64 requestId, string channelName, string userId, RTM_CHANNEL_ERROR_CODE errorCode)
    {
        string str = string.Format("OnJoinResult: requestId:{0} channelName:{1} userId:{2} errorCode:{3}", requestId, channelName, userId, errorCode);

        messageDisplay.AddMessage(str, Message.MessageType.Info);
    }

    public override void OnTopicSubscribed(UInt64 requestId, string channelName, string userId, string topic,
                                              UserList succeedUsers, UserList failedUsers, RTM_CHANNEL_ERROR_CODE errorCode)
    {
        messageDisplay.AddMessage("OnTopicSubscribed" + " requestId:" + requestId + " channelName:" + channelName + " userId :" + userId + " topic :" + topic + " succeedUsersCount :" + succeedUsers.userCount + " failedUsers :" + failedUsers.userCount, Message.MessageType.Info);

        for (int i = 0; i < succeedUsers.userCount; i++)
        {
            messageDisplay.AddMessage("OnTopicSubscribed succeedUsers index " + i + " UserName is " + succeedUsers.users[i], Message.MessageType.Info);
        }

        for (int i = 0; i < failedUsers.userCount; i++)
        {
            messageDisplay.AddMessage("OnTopicSubscribed failedUsers index " + i + " UserName is " + succeedUsers.users[i], Message.MessageType.Info);
        }

    }
    public override void OnConnectionStateChange(string channelName, RTM_CONNECTION_STATE state, RTM_CONNECTION_CHANGE_REASON reason)
    {
        messageDisplay.AddMessage("OnConnectionStateChange : " + channelName + " CONNECTION_STATE : " + state.ToString() + " RTM_CONNECTION_CHANGE_REASON : " + reason.ToString(), Message.MessageType.Info);
    }

    public override void OnJoinTopicResult(UInt64 requestId, string channelName, string userId, string topic, string meta, RTM_CHANNEL_ERROR_CODE errorCode)
    {
        messageDisplay.AddMessage("OnJoinTopicResult " + " requestId:" + requestId + " channelName:" + channelName + " userId :" + userId + " topic : " + topic + " meta : " + meta + " STREAM_CHANNEL_ERROR_CODE : " + errorCode.ToString(), Message.MessageType.Info);
    }

    public override void OnLeaveTopicResult(UInt64 requestId, string channelName, string userId, string topic, string meta, RTM_CHANNEL_ERROR_CODE errorCode)
    {
        messageDisplay.AddMessage("OnLeaveTopicResult " + " requestId:" + requestId + " channelName:" + channelName + " userId:" + userId + " topic" + topic + " meta" + meta + " STREAM_CHANNEL_ERROR_CODE" + errorCode.ToString(), Message.MessageType.Info);
    }
}
