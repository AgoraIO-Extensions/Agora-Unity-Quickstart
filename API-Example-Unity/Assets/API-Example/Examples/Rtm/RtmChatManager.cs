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

        //Send MessageChannel Message
        [SerializeField] InputField messageChannelNameInput, messageChannelMessageInput;
        //Lock
        [SerializeField] InputField LockChannelNameInput, LockNameInput, LockOwnerInput;
        //Presence
        [SerializeField] InputField PresenceChannelNameInput, PresenceKeyInput, PresenceValueInput, PresenceUserIdInput;
        //Storage
        [SerializeField] InputField MetadataMajorReversionInput, MetadataItemKeyInput, MetadataItemValueInput, MetadataItemAuthorUserIdInput, MetadataItemReversionIdInput, MetadataItemUpdateTsIdInput, MetadataChannelInput, MetadataUserIdInput, MetadataLockNameInput;

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

            config.setEventHandler(rtmEventHandler);

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

                int ret = streamChannel.PublishTopicMessage(topic, message, message.Length, publishOptions);

                messageDisplay.AddMessage("StreamChannel.PublishTopicMessage  ret:" + ret, Message.MessageType.Info);
            }
        }


        #region messageChannel

        public void SubscribeMessageChannel()
        {
            if (rtmClient != null)
            {

                string channelName = this.messageChannelNameInput.text;
                UInt64 requestId = 0;
                SubscribeOptions subscribeOptions = new SubscribeOptions()
                {
                    withMessage = true,
                    withMetadata = true,
                    withPresence = true,
                    withLock = true
                };

                int ret = rtmClient.Subscribe(channelName, subscribeOptions, ref requestId);

                messageDisplay.AddMessage("rtmClient.Subscribe  ret:" + ret + " requestId:" + requestId, Message.MessageType.Info);
            }
        }

        public void UnsubscribeMessageChannel()
        {
            if (rtmClient != null)
            {
                string channelName = this.messageChannelNameInput.text;

                int ret = rtmClient.Unsubscribe(channelName);

                messageDisplay.AddMessage("rtmClient.Unsubscribe  ret:" + ret, Message.MessageType.Info);
            }
        }

        public void SendMessageChannelMessage()
        {
            if (rtmClient != null)
            {
                string message = messageChannelMessageInput.text;
                string channelName = this.messageChannelNameInput.text;
                UInt64 requestId = 0;
                int ret = rtmClient.Publish(channelName, message, message.Length, new PublishOptions(), ref requestId);

                messageDisplay.AddMessage("rtmClient.Publish  ret:" + ret + " requestId:" + requestId, Message.MessageType.Info);
            }
        }

        #endregion

        #region Lock
        public void SetLock()
        {
            if (rtmClient != null)
            {
                IRtmLock rtmLock = rtmClient.GetLock();
                string channelName = this.LockChannelNameInput.text;
                string lockName = this.LockNameInput.text;
                UInt64 requestId = 0;
                int ret = rtmLock.SetLock(channelName, RTM_CHANNEL_TYPE.RTM_CHANNEL_TYPE_MESSAGE, lockName, 30, ref requestId);

                messageDisplay.AddMessage("IRtmLock.SetLock  ret:" + ret + " requestId:" + requestId, Message.MessageType.Info);
            }
        }

        public void GetLock()
        {
            if (rtmClient != null)
            {
                IRtmLock rtmLock = rtmClient.GetLock();
                string channelName = this.LockChannelNameInput.text;
                string lockName = this.LockNameInput.text;
                UInt64 requestId = 0;
                int ret = rtmLock.GetLocks(channelName, RTM_CHANNEL_TYPE.RTM_CHANNEL_TYPE_MESSAGE, ref requestId);

                messageDisplay.AddMessage("IRtmLock.GetLocks  ret:" + ret + " requestId:" + requestId, Message.MessageType.Info);
            }
        }

        public void RemoveLock()
        {
            if (rtmClient != null)
            {
                IRtmLock rtmLock = rtmClient.GetLock();
                string channelName = this.LockChannelNameInput.text;
                string lockName = this.LockNameInput.text;
                UInt64 requestId = 0;
                int ret = rtmLock.RemoveLock(channelName, RTM_CHANNEL_TYPE.RTM_CHANNEL_TYPE_MESSAGE, lockName, ref requestId);

                messageDisplay.AddMessage("IRtmLock.RemoveLock  ret:" + ret + " requestId:" + requestId, Message.MessageType.Info);
            }
        }

        public void AcquireLock()
        {
            if (rtmClient != null)
            {
                IRtmLock rtmLock = rtmClient.GetLock();
                string channelName = this.LockChannelNameInput.text;
                string lockName = this.LockNameInput.text;
                UInt64 requestId = 0;
                int ret = rtmLock.AcquireLock(channelName, RTM_CHANNEL_TYPE.RTM_CHANNEL_TYPE_MESSAGE, lockName, true, ref requestId);

                messageDisplay.AddMessage("IRtmLock.AcquireLock  ret:" + ret + " requestId:" + requestId, Message.MessageType.Info);
            }
        }

        public void ReleaseLock()
        {
            if (rtmClient != null)
            {
                IRtmLock rtmLock = rtmClient.GetLock();
                string channelName = this.LockChannelNameInput.text;
                string lockName = this.LockNameInput.text;
                UInt64 requestId = 0;
                int ret = rtmLock.ReleaseLock(channelName, RTM_CHANNEL_TYPE.RTM_CHANNEL_TYPE_MESSAGE, lockName, ref requestId);

                messageDisplay.AddMessage("IRtmLock.RemoveLock  ret:" + ret + " requestId:" + requestId, Message.MessageType.Info);
            }
        }

        public void RevokeLock()
        {
            if (rtmClient != null)
            {
                IRtmLock rtmLock = rtmClient.GetLock();
                string channelName = this.LockChannelNameInput.text;
                string lockName = this.LockNameInput.text;
                string owner = this.LockOwnerInput.text;
                UInt64 requestId = 0;
                int ret = rtmLock.RevokeLock(channelName, RTM_CHANNEL_TYPE.RTM_CHANNEL_TYPE_MESSAGE, lockName, owner, ref requestId);

                messageDisplay.AddMessage("IRtmLock.RemoveLock  ret:" + ret + " requestId:" + requestId, Message.MessageType.Info);
            }
        }

        #endregion

        #region Presence
        public void WhoNow()
        {
            if (rtmClient != null)
            {
                IRtmPresence rtmPresence = rtmClient.GetPresence();
                string channelName = this.PresenceChannelNameInput.text;
                string key = this.PresenceKeyInput.text;
                string value = this.PresenceValueInput.text;
                string userId = this.PresenceUserIdInput.text;
                PresenceOptions presenceOptions = new PresenceOptions()
                {
                    withState = true,
                    withUserId = true
                };
                UInt64 requestId = 0;

                int ret = rtmPresence.WhoNow(channelName, RTM_CHANNEL_TYPE.RTM_CHANNEL_TYPE_MESSAGE, presenceOptions, ref requestId);
                messageDisplay.AddMessage("IRtmPresence.WhoNow  ret:" + ret + " requestId:" + requestId, Message.MessageType.Info);
            }
        }

        public void WhereNow()
        {
            if (rtmClient != null)
            {
                IRtmPresence rtmPresence = rtmClient.GetPresence();
                string channelName = this.PresenceChannelNameInput.text;
                string key = this.PresenceKeyInput.text;
                string value = this.PresenceValueInput.text;
                string userId = this.PresenceUserIdInput.text;
                PresenceOptions presenceOptions = new PresenceOptions()
                {
                    withState = true,
                    withUserId = true
                };
                UInt64 requestId = 0;

                int ret = rtmPresence.WhereNow(userId, ref requestId);
                messageDisplay.AddMessage("IRtmPresence.WhereNow  ret:" + ret + " requestId:" + requestId, Message.MessageType.Info);
            }
        }

        public void SetState()
        {
            if (rtmClient != null)
            {
                IRtmPresence rtmPresence = rtmClient.GetPresence();
                string channelName = this.PresenceChannelNameInput.text;
                string key = this.PresenceKeyInput.text;
                string value = this.PresenceValueInput.text;
                string userId = this.PresenceUserIdInput.text;
                StateItem[] stateItems = new StateItem[1];
                stateItems[0] = new StateItem(key, value);
                UInt64 requestId = 0;

                int ret = rtmPresence.SetState(channelName, RTM_CHANNEL_TYPE.RTM_CHANNEL_TYPE_MESSAGE, stateItems, 1, ref requestId);
                messageDisplay.AddMessage("IRtmPresence.SetState  ret:" + ret + " requestId:" + requestId, Message.MessageType.Info);
            }
        }

        public void RemoveState()
        {
            if (rtmClient != null)
            {
                IRtmPresence rtmPresence = rtmClient.GetPresence();
                string channelName = this.PresenceChannelNameInput.text;
                string key = this.PresenceKeyInput.text;
                string value = this.PresenceValueInput.text;
                string userId = this.PresenceUserIdInput.text;
                string[] keys = new string[] { key };

                UInt64 requestId = 0;

                int ret = rtmPresence.RemoveState(channelName, RTM_CHANNEL_TYPE.RTM_CHANNEL_TYPE_MESSAGE, keys, 1, ref requestId);
                messageDisplay.AddMessage("IRtmPresence.RemoveState  ret:" + ret + " requestId:" + requestId, Message.MessageType.Info);
            }
        }

        public void GetState()
        {
            if (rtmClient != null)
            {
                IRtmPresence rtmPresence = rtmClient.GetPresence();
                string channelName = this.PresenceChannelNameInput.text;
                string key = this.PresenceKeyInput.text;
                string value = this.PresenceValueInput.text;
                string userId = this.PresenceUserIdInput.text;

                UInt64 requestId = 0;

                int ret = rtmPresence.GetState(channelName, RTM_CHANNEL_TYPE.RTM_CHANNEL_TYPE_MESSAGE, userId, ref requestId);
                messageDisplay.AddMessage("IRtmPresence.GetState  ret:" + ret + " requestId:" + requestId, Message.MessageType.Info);
            }
        }

        #endregion

        #region Storage

        public void SetChannelMetadata()
        {
            if (rtmClient != null)
            {
                IRtmStorage rtmStorage = rtmClient.GetStorage();
                string channelName = this.MetadataChannelInput.text;
                string userId = this.MetadataUserIdInput.text;
                MetadataOptions metadataOptions = new MetadataOptions()
                {
                    recordUserId = true,
                    recordTs = true
                };
                string lockName = this.MetadataLockNameInput.text;
                UInt64 requestId = 0;

                int ret = rtmStorage.SetChannelMetadata(channelName, RTM_CHANNEL_TYPE.RTM_CHANNEL_TYPE_MESSAGE, GetRtmMetadata(), metadataOptions, lockName, ref requestId);
                messageDisplay.AddMessage("IRtmStorage.SetChannelMetadata  ret:" + ret + " requestId:" + requestId, Message.MessageType.Info);
            }
        }

        public void UpdateChannelMetadata()
        {
            if (rtmClient != null)
            {
                IRtmStorage rtmStorage = rtmClient.GetStorage();
                string channelName = this.MetadataChannelInput.text;
                string userId = this.MetadataUserIdInput.text;
                MetadataOptions metadataOptions = new MetadataOptions()
                {
                    recordUserId = true,
                    recordTs = true
                };
                string lockName = this.MetadataLockNameInput.text;
                UInt64 requestId = 0;

                int ret = rtmStorage.UpdateChannelMetadata(channelName, RTM_CHANNEL_TYPE.RTM_CHANNEL_TYPE_MESSAGE, GetRtmMetadata(), metadataOptions, lockName, ref requestId);
                messageDisplay.AddMessage("IRtmStorage.UpdateChannelMetadata  ret:" + ret + " requestId:" + requestId, Message.MessageType.Info);
            }
        }

        public void RemoveChannelMetadata()
        {
            if (rtmClient != null)
            {
                IRtmStorage rtmStorage = rtmClient.GetStorage();
                string channelName = this.MetadataChannelInput.text;
                string userId = this.MetadataUserIdInput.text;
                MetadataOptions metadataOptions = new MetadataOptions()
                {
                    recordUserId = true,
                    recordTs = true
                };
                string lockName = this.MetadataLockNameInput.text;
                UInt64 requestId = 0;

                int ret = rtmStorage.RemoveChannelMetadata(channelName, RTM_CHANNEL_TYPE.RTM_CHANNEL_TYPE_MESSAGE, GetRtmMetadata(), metadataOptions, lockName, ref requestId);
                messageDisplay.AddMessage("IRtmStorage.RemoveChannelMetadata  ret:" + ret + " requestId:" + requestId, Message.MessageType.Info);
            }
        }

        public void GetChannelMetadata()
        {
            if (rtmClient != null)
            {
                IRtmStorage rtmStorage = rtmClient.GetStorage();
                string channelName = this.MetadataChannelInput.text;
                string userId = this.MetadataUserIdInput.text;
                MetadataOptions metadataOptions = new MetadataOptions()
                {
                    recordUserId = true,
                    recordTs = true
                };
                string lockName = this.MetadataLockNameInput.text;
                UInt64 requestId = 0;

                int ret = rtmStorage.GetChannelMetadata(channelName, RTM_CHANNEL_TYPE.RTM_CHANNEL_TYPE_MESSAGE, ref requestId);
                messageDisplay.AddMessage("IRtmStorage.GetChannelMetadata  ret:" + ret + " requestId:" + requestId, Message.MessageType.Info);
            }
        }

        public void SetUserMetadata()
        {
            if (rtmClient != null)
            {
                IRtmStorage rtmStorage = rtmClient.GetStorage();
                string channelName = this.MetadataChannelInput.text;
                string userId = this.MetadataUserIdInput.text;
                MetadataOptions metadataOptions = new MetadataOptions()
                {
                    recordUserId = true,
                    recordTs = true
                };
                string lockName = this.MetadataLockNameInput.text;
                UInt64 requestId = 0;

                int ret = rtmStorage.SetUserMetadata(userId, GetRtmMetadata(), metadataOptions, ref requestId);
                messageDisplay.AddMessage("IRtmStorage.SetUserMetadata  ret:" + ret + " requestId:" + requestId, Message.MessageType.Info);
            }
        }

        public void UpdateUserMetadata()
        {
            if (rtmClient != null)
            {
                IRtmStorage rtmStorage = rtmClient.GetStorage();
                string channelName = this.MetadataChannelInput.text;
                string userId = this.MetadataUserIdInput.text;
                MetadataOptions metadataOptions = new MetadataOptions()
                {
                    recordUserId = true,
                    recordTs = true
                };
                string lockName = this.MetadataLockNameInput.text;
                UInt64 requestId = 0;

                int ret = rtmStorage.UpdateUserMetadata(userId, GetRtmMetadata(), metadataOptions, ref requestId);
                messageDisplay.AddMessage("IRtmStorage.UpdateUserMetadata  ret:" + ret + " requestId:" + requestId, Message.MessageType.Info);
            }
        }

        public void RemoveUserMetadata()
        {
            if (rtmClient != null)
            {
                IRtmStorage rtmStorage = rtmClient.GetStorage();
                string channelName = this.MetadataChannelInput.text;
                string userId = this.MetadataUserIdInput.text;
                MetadataOptions metadataOptions = new MetadataOptions()
                {
                    recordUserId = true,
                    recordTs = true
                };
                string lockName = this.MetadataLockNameInput.text;
                UInt64 requestId = 0;

                int ret = rtmStorage.RemoveUserMetadata(userId, GetRtmMetadata(), metadataOptions, ref requestId);
                messageDisplay.AddMessage("IRtmStorage.RemoveUserMetadata  ret:" + ret + " requestId:" + requestId, Message.MessageType.Info);
            }
        }

        public void GetUserMetadata()
        {
            if (rtmClient != null)
            {
                IRtmStorage rtmStorage = rtmClient.GetStorage();
                string channelName = this.MetadataChannelInput.text;
                string userId = this.MetadataUserIdInput.text;
                MetadataOptions metadataOptions = new MetadataOptions()
                {
                    recordUserId = true,
                    recordTs = true
                };
                string lockName = this.MetadataLockNameInput.text;
                UInt64 requestId = 0;

                int ret = rtmStorage.GetUserMetadata(userId, ref requestId);
                messageDisplay.AddMessage("IRtmStorage.GetUserMetadata  ret:" + ret + " requestId:" + requestId, Message.MessageType.Info);
            }
        }

        public void SubscribeUserMetadata()
        {
            if (rtmClient != null)
            {
                IRtmStorage rtmStorage = rtmClient.GetStorage();
                string channelName = this.MetadataChannelInput.text;
                string userId = this.MetadataUserIdInput.text;
                MetadataOptions metadataOptions = new MetadataOptions()
                {
                    recordUserId = true,
                    recordTs = true
                };
                string lockName = this.MetadataLockNameInput.text;
                UInt64 requestId = 0;

                int ret = rtmStorage.SubscribeUserMetadata(userId);
                messageDisplay.AddMessage("IRtmStorage.SubscribeUserMetadata  ret:" + ret, Message.MessageType.Info);
            }
        }

        public void UnsubscribeUserMetadata()
        {
            if (rtmClient != null)
            {
                IRtmStorage rtmStorage = rtmClient.GetStorage();
                string channelName = this.MetadataChannelInput.text;
                string userId = this.MetadataUserIdInput.text;
                MetadataOptions metadataOptions = new MetadataOptions()
                {
                    recordUserId = true,
                    recordTs = true
                };
                string lockName = this.MetadataLockNameInput.text;
                UInt64 requestId = 0;

                int ret = rtmStorage.UnsubscribeUserMetadata(userId);
                messageDisplay.AddMessage("IRtmStorage.UnsubscribeUserMetadata  ret:" + ret, Message.MessageType.Info);
            }
        }

        private RtmMetadata GetRtmMetadata()
        {
            RtmMetadata rtmMetadata = new RtmMetadata();
            rtmMetadata.majorRevision = long.Parse(this.MetadataMajorReversionInput.text);
            MetadataItem metadataItem = new MetadataItem()
            {
                key = this.MetadataItemKeyInput.text,
                value = this.MetadataItemValueInput.text,
                authorUserId = this.MetadataItemAuthorUserIdInput.text,
                revision = long.Parse(this.MetadataItemReversionIdInput.text),
                updateTs = long.Parse(this.MetadataItemUpdateTsIdInput.text)
            };
            rtmMetadata.metadataItems = new MetadataItem[] { metadataItem };
            rtmMetadata.metadataItemsSize = 1;
            return rtmMetadata;
        }

        #endregion


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

    #region Lock

    public override void OnLockEvent(LockEvent @event)
    {
        string info = string.Format("OnLockEvent channelType:{0}, eventType:{1}, channelName:{2}, count:{3}", @event.channelType, @event.eventType, @event.channelName, @event.count);
        messageDisplay.AddMessage(info, Message.MessageType.Info);
        if (@event.count > 0)
        {
            for (int i = 0; i < @event.lockDetailList.Length; i++)
            {
                var detail = @event.lockDetailList[i];
                string info2 = string.Format("lockDetailList lockName:{0}, owner:{1}, ttl:{2}", detail.lockName, detail.owner, detail.ttl);
                messageDisplay.AddMessage(info2, Message.MessageType.Info);
            }
        }

    }

    public override void OnSetLockResult(UInt64 requestId, string channelName, RTM_CHANNEL_TYPE channelType,
                                            string lockName, OPERATION_ERROR_CODE errorCode)
    {
        string info = string.Format("OnSetLockResult requestId:{0},channelName:{1},channelType:{2},lockName:{3},errorCode:{4}", requestId, channelName, channelType, lockName, errorCode);
        messageDisplay.AddMessage(info, Message.MessageType.Info);
    }

    public override void OnRemoveLockResult(UInt64 requestId, string channelName, RTM_CHANNEL_TYPE channelType,
                                            string lockName, OPERATION_ERROR_CODE errorCode)
    {
        string info = string.Format("OnRemoveLockResult requestId:{0},channelName:{1},channelType:{2},lockName:{3},errorCode:{4}", requestId, channelName, channelType, lockName, errorCode);
        messageDisplay.AddMessage(info, Message.MessageType.Info);
    }

    public override void OnReleaseLockResult(UInt64 requestId, string channelName, RTM_CHANNEL_TYPE channelType,
                                            string lockName, OPERATION_ERROR_CODE errorCode)
    {
        string info = string.Format("OnReleaseLockResult requestId:{0},channelName:{1},channelType:{2},lockName:{3},errorCode:{4}", requestId, channelName, channelType, lockName, errorCode);
        messageDisplay.AddMessage(info, Message.MessageType.Info);
    }

    public override void OnAcquireLockResult(UInt64 requestId, string channelName, RTM_CHANNEL_TYPE channelType,
                                            string lockName, OPERATION_ERROR_CODE errorCode, string errorDetails)
    {
        string info = string.Format("OnAcquireLockResult requestId:{0},channelName:{1},channelType:{2},lockName:{3},errorCode:{4}", requestId, channelName, channelType, lockName, errorCode);
        messageDisplay.AddMessage(info, Message.MessageType.Info);
    }

    public override void OnRevokeLockResult(UInt64 requestId, string channelName, RTM_CHANNEL_TYPE channelType,
                                            string lockName, OPERATION_ERROR_CODE errorCode)
    {
        string info = string.Format("OnRevokeLockResult requestId:{0},channelName:{1},channelType:{2},lockName:{3},errorCode:{4}", requestId, channelName, channelType, lockName, errorCode);
        messageDisplay.AddMessage(info, Message.MessageType.Info);
    }

    public override void OnGetLocksResult(UInt64 requestId, string channelName, RTM_CHANNEL_TYPE channelType,
                                            LockDetail[] lockDetailList, UInt64 count, OPERATION_ERROR_CODE errorCode)
    {
        string info = string.Format("OnGetLocksResult requestId:{0},channelName:{1},channelType:{2},count:{3},errorCode:{4},", requestId, channelName, channelType, count, errorCode);
        messageDisplay.AddMessage(info, Message.MessageType.Info);
        if (count > 0)
        {
            for (int i = 0; i < lockDetailList.Length; i++)
            {
                var detail = lockDetailList[i];
                string info2 = string.Format("lockDetailList lockName:{0}, owner:{1}, ttl:{2}", detail.lockName, detail.owner, detail.ttl);
                messageDisplay.AddMessage(info2, Message.MessageType.Info);
            }
        }

    }
    #endregion

    #region Presence
    public override void WhoNowResult(UInt64 requestId, UserState[] userStateList, UInt64 count, OPERATION_ERROR_CODE errorCode)
    {

        string info = string.Format("WhoNowResult requestId:{0},count:{1},errorCode:{2},", requestId, count, errorCode);
        messageDisplay.AddMessage(info, Message.MessageType.Info);
        if (count > 0)
        {
            for (int i = 0; i < userStateList.Length; i++)
            {
                var userState = userStateList[i];
                string info2 = string.Format("userStateList userId:{0}, stateCount:{1}", userState.userId, userState.statesCount);
                messageDisplay.AddMessage(info2, Message.MessageType.Info);
            }
        }
    }

    public override void WhereNowResult(UInt64 requestId, ChannelInfo[] channels, UInt64 count, OPERATION_ERROR_CODE errorCode)
    {

        string info = string.Format("WhereNowResult requestId:{0},count:{1},errorCode:{2},", requestId, count, errorCode);
        messageDisplay.AddMessage(info, Message.MessageType.Info);
        if (count > 0)
        {
            for (int i = 0; i < channels.Length; i++)
            {
                var channelInfo = channels[i];
                string info2 = string.Format("userStateList channelName:{0}, channelType:{1}", channelInfo.channelName, channelInfo.channelType);
                messageDisplay.AddMessage(info2, Message.MessageType.Info);
            }
        }
    }

    public override void OnPresenceSetStateResult(UInt64 requestId, OPERATION_ERROR_CODE errorCode)
    {
        string info = string.Format("OnPresenceSetStateResult requestId:{0},errorCode:{1},", requestId, errorCode);
        messageDisplay.AddMessage(info, Message.MessageType.Info);
    }

    public override void OnPresenceRemoveStateResult(UInt64 requestId, OPERATION_ERROR_CODE errorCode)
    {
        string info = string.Format("OnPresenceRemoveStateResult requestId:{0},errorCode:{1},", requestId, errorCode);
        messageDisplay.AddMessage(info, Message.MessageType.Info);
    }

    public override void OnPresenceGetStateResult(UInt64 requestId, UserState state, OPERATION_ERROR_CODE errorCode)
    {
        string info = string.Format("OnPresenceGetStateResult requestId:{0},errorCode:{1},", requestId, errorCode);
        messageDisplay.AddMessage(info, Message.MessageType.Info);

        string info2 = string.Format("userStateList userId:{0}, stateCount:{1}", state.userId, state.statesCount);
        messageDisplay.AddMessage(info2, Message.MessageType.Info);
    }
    #endregion


    #region IRtmStorage
    public override void OnSetChannelMetadataResult(UInt64 requestId, string channelName, RTM_CHANNEL_TYPE channelType, OPERATION_ERROR_CODE errorCode)
    {
        string info = string.Format("OnSetChannelMetadataResult requestId:{0},channelName:{1},channelType:{2},errorCode:{3}", requestId, channelName, channelType, errorCode);
        messageDisplay.AddMessage(info, Message.MessageType.Info);
    }

    public override void OnUpdateChannelMetadataResult(UInt64 requestId, string channelName, RTM_CHANNEL_TYPE channelType, OPERATION_ERROR_CODE errorCode)
    {
        string info = string.Format("OnUpdateChannelMetadataResult requestId:{0},channelName:{1},channelType:{2},errorCode:{3}", requestId, channelName, channelType, errorCode);
        messageDisplay.AddMessage(info, Message.MessageType.Info);
    }

    public override void OnRemoveChannelMetadataResult(UInt64 requestId, string channelName, RTM_CHANNEL_TYPE channelType, OPERATION_ERROR_CODE errorCode)
    {
        string info = string.Format("OnRemoveChannelMetadataResult requestId:{0},channelName:{1},channelType:{2},errorCode:{3}", requestId, channelName, channelType, errorCode);
        messageDisplay.AddMessage(info, Message.MessageType.Info);
    }

    public override void OnGetChannelMetadataResult(UInt64 requestId, string channelName, RTM_CHANNEL_TYPE channelType, RtmMetadata data,
                                                      OPERATION_ERROR_CODE errorCode)
    {
        string info = string.Format("OnGetChannelMetadataResult requestId:{0},channelName:{1},channelType:{2},errorCode:{3}", requestId, channelName, channelType, errorCode);
        messageDisplay.AddMessage(info, Message.MessageType.Info);
        DisplayRtmMetadata(ref data);
    }

    public override void OnSetUserMetadataResult(UInt64 requestId, string userId, OPERATION_ERROR_CODE errorCode)
    {
        string info = string.Format("OnGetChannelMetadataResult requestId:{0},userId:{1},errorCode:{2}", requestId, userId, errorCode);
        messageDisplay.AddMessage(info, Message.MessageType.Info);
    }

    public override void OnUpdateUserMetadataResult(UInt64 requestId, string userId, OPERATION_ERROR_CODE errorCode)
    {
        string info = string.Format("OnGetChannelMetadataResult requestId:{0},userId:{1},errorCode:{2}", requestId, userId, errorCode);
        messageDisplay.AddMessage(info, Message.MessageType.Info);
    }

    public override void OnRemoveUserMetadataResult(UInt64 requestId, string userId, OPERATION_ERROR_CODE errorCode)
    {
        string info = string.Format("OnGetChannelMetadataResult requestId:{0},userId:{1},errorCode:{2}", requestId, userId, errorCode);
        messageDisplay.AddMessage(info, Message.MessageType.Info);
    }

    public override void OnGetUserMetadataResult(UInt64 requestId, string userId, RtmMetadata data, OPERATION_ERROR_CODE errorCode)
    {
        string info = string.Format("OnGetChannelMetadataResult requestId:{0},userId:{1},errorCode:{2}", requestId, userId, errorCode);
        messageDisplay.AddMessage(info, Message.MessageType.Info);
        DisplayRtmMetadata(ref data);
    }

    public override void OnSubscribeUserMetadataResult(string userId, OPERATION_ERROR_CODE errorCode)
    {
        string info = string.Format("OnGetChannelMetadataResult userId:{0},errorCode:{1}", userId, errorCode);
        messageDisplay.AddMessage(info, Message.MessageType.Info);
    }

    private void DisplayRtmMetadata(ref RtmMetadata data)
    {
        messageDisplay.AddMessage("RtmMetadata.majorRevision:" + data.majorRevision, Message.MessageType.Info);
        if (data.metadataItemsSize > 0)
        {
            foreach (var item in data.metadataItems)
            {
                messageDisplay.AddMessage(string.Format("key:{0},value:{1},authorUserId:{2},revision:{3},updateTs:{4}", item.key, item.value, item.authorUserId, item.revision, item.updateTs), Message.MessageType.Info);
            }
        }
    }
    #endregion
}
