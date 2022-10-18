using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Agora.Rtm;
using Agora.Rtc;
using io.agora.rtm.demo;

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
        [SerializeField] InputField TopicMsgInputBox;
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

        private void Awake()
        {
            userNameInput.text = PlayerPrefs.GetString("RTM_USER", "");
            channelNameInput.text = PlayerPrefs.GetString("RTM_CHANNEL", "");

            rtcEngine = RtcEngine.CreateAgoraRtcEngine();
            if (rtcEngine != null)
            {
                int init = rtcEngine.Initialize(new RtcEngineContext(appId, 0,
                                CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING,
                                AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT, AREA_CODE.AREA_CODE_CN, new Agora.Rtc.LogConfig("./log.txt")));
                rtcEngine.SetParameters("{\"rtc.vos_list\":[\"114.236.138.120:4052\"]}");
                messageDisplay.AddMessage("rtcEngine.Initialize + ret:" + init, Message.MessageType.Info);
            }

        }

        private void Start()
        {
            ShowDisplayTexts();
        }
        private void OnDestroy()
        {
            if (streamChannel != null)
            {
                var ret = streamChannel.Leave();
                streamChannel.Release();
                messageDisplay.AddMessage("StreamChannel.Leave + ret:" + ret, Message.MessageType.Info);
            }
            if (rtmClient != null)
            {
                rtmClient.Release();
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

        public void Join()
        {
            ChannelName = channelNameInput.text;
            if (rtmClient != null)
            {
                if(streamChannel ==null)
                {
                    streamChannel = rtmClient.CreateStreamChannel(ChannelName);
                }

                JoinChannelOptions options = new JoinChannelOptions();
                options.token = token;
                int ret = streamChannel.Join(options);
                messageDisplay.AddMessage("StreamChannel.Join + ret:" + ret, Message.MessageType.Info);
            }
        }
        public void ChannelLeave()
        {
            if (streamChannel != null)
            {
                int ret = streamChannel.Leave();

                messageDisplay.AddMessage("StreamChannel.ChannelLeave ret:" + ret, Message.MessageType.Info);
            }
        }

        public void ChannelDispose()
        {
            if (rtmClient != null && streamChannel != null)
            {
                streamChannel.Release();
            }

        }

        public void RtmDispose()
        {
            if (rtmClient != null)
            {
                ChannelDispose();
                rtmClient.Release();
                rtmClient = null;
            }
        }

        public void JoinTopic()
        {
            topic = TopicNameBox.text;

            JoinTopicOptions joinTopicOptions = new JoinTopicOptions();

            if (streamChannel != null)
            {
                int ret = streamChannel.JoinTopic(topic, joinTopicOptions);

                messageDisplay.AddMessage("StreamChannel.GetChannelName ret:" + streamChannel.GetChannelName(), Message.MessageType.Info);

                messageDisplay.AddMessage("StreamChannel.JoinTopic ret:" + ret, Message.MessageType.Info);
            }

        }

        public void LeaveTopic()
        {
            if (streamChannel != null)
            {
                int ret = streamChannel.LeaveTopic(topic);

                messageDisplay.AddMessage("StreamChannel.LeaveTopic ret:" + ret, Message.MessageType.Info);
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

            int ret = streamChannel.SubscribeTopic(subtopic, topicOptions);

            messageDisplay.AddMessage("StreamChannel.SubscribeTopic ret:" + ret, Message.MessageType.Info);

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
            UserList userList = new UserList();

            if (streamChannel != null)
            {
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
                int ret = streamChannel.PublishTopicMessage(topic, message);

                messageDisplay.AddMessage("StreamChannel.PublishTopicMessage  ret:" + ret, Message.MessageType.Info);
            }
        }

        public void SendTopicMessageStr()
        {
            if (streamChannel != null)
            {
                int ret = streamChannel.PublishTopicMessage(topic, TopicMessageBox.text);

                messageDisplay.AddMessage("StreamChannel.PublishTopicMessage  ret:" + ret, Message.MessageType.Info);
            }
        }

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

        bool ShowDisplayTexts()
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
        messageDisplay.AddMessage("OnPresenceEvent " + @event.channelName + " userId : " + @event.userId + " topicInfoNumber : " + @event.topicInfoNumber + " type : " + @event.type.ToString() + " channelType : " + @event.channelType.ToString(), Message.MessageType.Info);
    }

    public override void OnJoinResult(string channelName, string userId, STREAM_CHANNEL_ERROR_CODE errorCode)
    {
        messageDisplay.AddMessage("OnJoinResult " + channelName + " uid :" + userId + " STREAM_CHANNEL_ERROR_CODE : " + errorCode, Message.MessageType.Info);
    }

    public override void OnLeaveResult(string channelName, string userId, STREAM_CHANNEL_ERROR_CODE errorCode)
    {
        messageDisplay.AddMessage("OnLeaveResult " + channelName + " uid :" + userId + " STREAM_CHANNEL_ERROR_CODE : " + errorCode, Message.MessageType.Info);
    }

    public override void OnTopicSubscribed(string channelName, string userId, string topic, UserList succeedUsers, UserList failedUsers, STREAM_CHANNEL_ERROR_CODE errorCode)
    {
        messageDisplay.AddMessage("OnTopicSubscribed " + channelName + " userId :" + userId + " topic :" + topic + " succeedUsersCount :" + succeedUsers.userCount + " failedUsers :" + failedUsers.userCount, Message.MessageType.Info);

        for (int i = 0; i < succeedUsers.userCount; i++)
        {
            messageDisplay.AddMessage("OnTopicSubscribed succeedUsers index " + i + " UserName is " + succeedUsers.users[i], Message.MessageType.Info);
        }

        for (int i = 0; i < failedUsers.userCount; i++)
        {
            messageDisplay.AddMessage("OnTopicSubscribed failedUsers index " + i + " UserName is " + succeedUsers.users[i], Message.MessageType.Info);
        }

    }

    public override void OnTopicUnsubscribed(string channelName, string userId, string topic, UserList succeedUsers, UserList failedUsers, STREAM_CHANNEL_ERROR_CODE errorCode)
    {
        messageDisplay.AddMessage("OnTopicUnsubscribed " + channelName + " userId :" + userId + " topic :" + topic + " succeedUsersCount :" + succeedUsers.userCount + " failedUsers :" + failedUsers.userCount, Message.MessageType.Info);

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

    public override void OnJoinTopicResult(string channelName, string userId, string topic, string meta, STREAM_CHANNEL_ERROR_CODE errorCode)
    {
        messageDisplay.AddMessage("OnJoinTopicResult " + channelName + " userId :" + userId + " topic : " + topic + " meta : " + meta + " STREAM_CHANNEL_ERROR_CODE : " + errorCode.ToString(), Message.MessageType.Info);
    }

    public override void OnLeaveTopicResult(string channelName, string userId, string topic, string meta, STREAM_CHANNEL_ERROR_CODE errorCode)
    {
        messageDisplay.AddMessage("OnLeaveTopicResult " + channelName + " userId:" + userId + " topic" + topic + " meta" + meta + " STREAM_CHANNEL_ERROR_CODE" + errorCode.ToString(), Message.MessageType.Info);
    }
}
