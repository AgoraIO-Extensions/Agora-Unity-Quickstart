using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Agora.Rtm;
using Agora.Rtc;
using io.agora.rtm.demo;
using System;
using System.Threading.Tasks;

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

        [SerializeField] InputField userNameInput, presenceTimeoutInput, channelNameInput;

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
        private RtmEventHandler rtmEventHandler;

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

        private async void OnDestroy()
        {
            if (streamChannel != null)
            {

                RtmResult<LeaveResult> rtmResult = await streamChannel.LeaveAsync();
                streamChannel.Dispose();
                messageDisplay.AddMessage("StreamChannel.Leave + ret:" + rtmResult.Status.ErrorCode, Message.MessageType.Info);
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
        public async void Initialize()
        {
            appId = appId == "" ? AppIdInputBox.text : appId;
            token = token == "" ? TokenInputBox.text : token;
            uint presenceTimeout = uint.Parse(presenceTimeoutInput.text); 
            UserName = userNameInput.text;
            if (string.IsNullOrEmpty(UserName) || string.IsNullOrEmpty(appId))
            {
                Debug.LogError("We need a username and appId to login");
                return;
            }

           
            RtmConfig config = new RtmConfig();
            config.appId = appId;
            config.userId = UserName;
            config.presenceTimeout = presenceTimeout;
            try
            {
                rtmClient = RtmClient.CreateAgoraRtmClient(config);
            }
            catch (RTMException e)
            {
                messageDisplay.AddMessage("rtmClient.init error + ret:" + e.Status.ErrorCode, Message.MessageType.Error);
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


                var ret = rtmClient.SetParameters("{\"rtm.link_address0\":[\"183.131.160.141\", 9130]}");
                messageDisplay.AddMessage("rtmClient.SetParameters + ret:" + ret, Message.MessageType.Info);
                ret = rtmClient.SetParameters("{\"rtm.link_address1\":[\"183.131.160.142\", 9131]}");
                messageDisplay.AddMessage("rtmClient.SetParameters + ret:" + ret, Message.MessageType.Info);
                ret = rtmClient.SetParameters("{\"rtm.link_encryption\": false}");
                messageDisplay.AddMessage("rtmClient.SetParameters + ret:" + ret, Message.MessageType.Info);
                //ret = rtmClient.SetParameters("{\"rtm.ap_address\":[\"114.236.137.40\", 8443]}");
                //messageDisplay.AddMessage("rtmClient.SetParameters + ret:" + ret, Message.MessageType.Info);

                var result = await rtmClient.LoginAsync(token);

                if (result.Status.Error)
                {
                    messageDisplay.AddMessage("rtmClient.Login + ret:" + result.Status.ErrorCode, Message.MessageType.Info);
                }
                else
                {
                    messageDisplay.AddMessage("rtmClient.Login + respones:" + result.Response.ErrorCode, Message.MessageType.Info);
                }


            }
        }

        public async void JoinChannel()
        {
            ChannelName = channelNameInput.text;
            if (rtmClient != null)
            {
                if (streamChannel == null)
                {
                    streamChannel = rtmClient.CreateStreamChannel(ChannelName);
                }

                JoinChannelOptions options = new JoinChannelOptions();
                options.withLock = true;
                options.withMetadata = true;
                options.withPresence = true;

                options.token = TokenInputBox.text;
                if (options.token == "") options.token = AppIdInputBox.text;
                if (streamChannel != null)
                {

                    var result = await streamChannel.JoinAsync(options);
                    if (result.Status.Error)
                    {
                        messageDisplay.AddMessage(string.Format("StreamChannel.Join Status.ErrorCode:{0} ", result.Status.ErrorCode), Message.MessageType.Info);
                    }
                    else
                    {
                        string str = string.Format("StreamChannel.Join Response: channelName:{0} userId:{1} errorCode:{2}",
                            result.Response.ChannelName, result.Response.UserId, result.Response.ErrorCode);
                        messageDisplay.AddMessage(str, Message.MessageType.Info);
                    }

                }
                else
                {
                    messageDisplay.AddMessage("StreamChannel is invalid: ChannelName is invalid", Message.MessageType.Error);
                }
            }
        }
        public async void ChannelLeave()
        {
            if (streamChannel != null)
            {

                var result = await streamChannel.LeaveAsync();

                if (result.Status.Error)
                {
                    messageDisplay.AddMessage(string.Format("StreamChannel.Leave Status.ErrorCode:{0} ", result.Status.ErrorCode), Message.MessageType.Info);
                }
                else
                {
                    string str = string.Format("StreamChannel.Leave Response: channelName:{0} userId:{1} errorCode:{2}",
                        result.Response.ChannelName, result.Response.UserId, result.Response.ErrorCode);
                    messageDisplay.AddMessage(str, Message.MessageType.Info);
                }
            }
        }

        public void ChannelDispose()
        {
            if (rtmClient != null && streamChannel != null)
            {
                var ret = streamChannel.Dispose();
                messageDisplay.AddMessage(string.Format("StreamChannel.Dispose ret:{0}", ret), Message.MessageType.Info);
                streamChannel = null;
            }
        }

        public void RtmDispose()
        {
            if (rtmClient != null)
            {
                ChannelDispose();
                var ret = rtmClient.Logout();
                messageDisplay.AddMessage(string.Format("RtmClient.Logout ret:{0} ", ret), Message.MessageType.Info);

                rtmClient.Dispose();
                rtmClient = null;
            }
        }

        public async void JoinTopic()
        {
            topic = TopicNameBox.text;

            JoinTopicOptions joinTopicOptions = new JoinTopicOptions();
            joinTopicOptions.syncWithMedia = false;
            if (streamChannel != null)
            {

                messageDisplay.AddMessage("StreamChannel.JoinTopic ret:" + streamChannel.GetChannelName(), Message.MessageType.Info);
                var result = await streamChannel.JoinTopicAsync(topic, joinTopicOptions);
                if (result.Status.Error)
                {
                    messageDisplay.AddMessage(string.Format("StreamChannel.JoinTopic Status.ErrorCode:{0} ", result.Status.ErrorCode), Message.MessageType.Info);
                }
                else
                {
                    string str = string.Format("StreamChannel.JoinTopic Response: channelName:{0} userId:{1} topic:{2} meta:{3} errorCode:{4}",
                      result.Response.ChannelName, result.Response.UserId, result.Response.Topic, result.Response.Meta, result.Response.ErrorCode);
                    messageDisplay.AddMessage(str, Message.MessageType.Info);
                }
            }

        }

        public async void LeaveTopic()
        {
            if (streamChannel != null)
            {

                var result = await streamChannel.LeaveTopicAsync(topic);

                if (result.Status.Error)
                {
                    messageDisplay.AddMessage(string.Format("StreamChannel.LeaveTopic Status.ErrorCode:{0} ", result.Status.ErrorCode), Message.MessageType.Info);
                }
                else
                {
                    string str = string.Format("StreamChannel.LeaveTopic Response: channelName:{0} userId:{1} topic:{2} meta:{3} errorCode:{4}",
                      result.Response.ChannelName, result.Response.UserId, result.Response.Topic, result.Response.Meta, result.Response.ErrorCode);
                    messageDisplay.AddMessage(str, Message.MessageType.Info);
                }
            }
        }


        public async void TopicSubscribed()
        {
            subtopic = TopicSubscribedBox.text;

            TopicOptions topicOptions = new TopicOptions();

            if (userList != null && userList.Count > 0)
            {
                topicOptions.users = userList.ToArray();
                topicOptions.userCount = (uint)userList.Count;
            }

            var result = await streamChannel.SubscribeTopicAsync(subtopic, topicOptions);

            if (result.Status.Error)
            {
                messageDisplay.AddMessage(string.Format("StreamChannel.SubscribeTopic Status.ErrorCode:{0} ", result.Status.ErrorCode), Message.MessageType.Info);
            }
            else
            {
                messageDisplay.AddMessage("StreamChannel.SubscribeTopic Response:" + " channelName:" + result.Response.ChannelName + " userId :" + result.Response.UserId + " topic :" + result.Response.Topic + " succeedUsersCount :" + result.Response.SucceedUsers.userCount + " failedUsers :" + result.Response.FailedUsers.userCount, Message.MessageType.Info);

                for (int i = 0; i < result.Response.SucceedUsers.userCount; i++)
                {
                    messageDisplay.AddMessage("StreamChannel.SubscribeTopic Response succeedUsers index " + i + " UserName is " + result.Response.SucceedUsers.users[i], Message.MessageType.Info);
                }

                for (int i = 0; i < result.Response.FailedUsers.userCount; i++)
                {
                    messageDisplay.AddMessage("StreamChannel.SubscribeTopic Response failedUsers index " + i + " UserName is " + result.Response.FailedUsers.users[i], Message.MessageType.Info);
                }
            }

            userList.Clear();
        }

        public void TopicUnSubscribed()
        {
            subtopic = TopicSubscribedBox.text;
            TopicOptions topicOptions = new TopicOptions();

            if (userList != null && userList.Count > 0)
            {
                topicOptions.users = userList.ToArray();
                topicOptions.userCount = (uint)userList.Count;
            }

            var ret = streamChannel.UnsubscribeTopicAsync(subtopic, topicOptions);

            messageDisplay.AddMessage("StreamChannel.UnsubscribeTopic ret:" + ret, Message.MessageType.Info);
        }

        public async void GetSubscribedUserList()
        {
            if (streamChannel != null)
            {
                subtopic = TopicSubscribedBox.text;

                var result = await streamChannel.GetSubscribedUserListAsync(subtopic);
                if (result.Status.Error)
                {
                    messageDisplay.AddMessage("StreamChannel.GetSubscribedTopic ErrorCode:" + result.Status.ErrorCode, Message.MessageType.Info);
                }
                else
                {
                    var userList = result.Response.Users;
                    for (int i = 0; i < userList.userCount; i++)
                    {
                        messageDisplay.AddMessage("userIndex : " + i + " userListName : " + userList.users[i], Message.MessageType.Info);
                    }
                }
            }

        }

        public async void SendTopicMessage()
        {
            topic = TopicNameBox.text;
            byte[] message = System.Text.Encoding.Default.GetBytes(TopicMessageBox.text);
            if (streamChannel != null)
            {
                PublishOptions publishOptions = new PublishOptions();
                publishOptions.type = RTM_MESSAGE_TYPE.RTM_MESSAGE_TYPE_STRING;
                publishOptions.sendTs = 0;

                var result = await streamChannel.PublishTopicMessageAsync(topic, message, publishOptions);

                messageDisplay.AddMessage("StreamChannel.PublishTopicMessage  ret:" + result.Status.ErrorCode, Message.MessageType.Info);
            }
        }


        #region messageChannel

        public async void SubscribeMessageChannel()
        {
            if (rtmClient != null)
            {

                string channelName = this.messageChannelNameInput.text;
                SubscribeOptions subscribeOptions = new SubscribeOptions()
                {
                    withMessage = true,
                    withMetadata = true,
                    withPresence = true,
                    withLock = true
                };

                var result = await rtmClient.SubscribeAsync(channelName, subscribeOptions);

                if (result.Status.Error)
                {
                    messageDisplay.AddMessage(string.Format("rtmClient.Subscribe Status.ErrorCode:{0} ", result.Status.ErrorCode), Message.MessageType.Info);
                }
                else
                {
                    string info = string.Format("rtmClient.Subscribe Response , channelName:{0}, errorCode:{1}", result.Response.ChannelName, result.Response.ErrorCode);
                    messageDisplay.AddMessage(info, Message.MessageType.Info);
                }
            }
        }

        public async void UnsubscribeMessageChannel()
        {
            if (rtmClient != null)
            {
                string channelName = this.messageChannelNameInput.text;

                var result = await rtmClient.UnsubscribeAsync(channelName);

                messageDisplay.AddMessage("rtmClient.Unsubscribe  ret:" + result.Status.ErrorCode, Message.MessageType.Info);
            }
        }

        public async void SendMessageChannelMessage()
        {
            if (rtmClient != null)
            {
                string message = messageChannelMessageInput.text;
                string channelName = this.messageChannelNameInput.text;

                var result = await rtmClient.PublishAsync(channelName, message, new PublishOptions());


                if (result.Status.Error)
                {
                    messageDisplay.AddMessage(string.Format("rtmClient.Publish Status.ErrorCode:{0} ", result.Status.ErrorCode), Message.MessageType.Info);
                }
                else
                {
                    string info = string.Format("rtmClient.Publish Response , errorCode:{0}", result.Response.ErrorCode);
                    messageDisplay.AddMessage(info, Message.MessageType.Info);
                }
            }
        }

        #endregion

        #region Lock
        public async void SetLock()
        {
            if (rtmClient != null)
            {
                IRtmLock rtmLock = rtmClient.GetLock();
                string channelName = this.LockChannelNameInput.text;
                string lockName = this.LockNameInput.text;

                var result = await rtmLock.SetLockAsync(channelName, RTM_CHANNEL_TYPE.RTM_CHANNEL_TYPE_MESSAGE, lockName, 30);

                if (result.Status.Error)
                {
                    messageDisplay.AddMessage(string.Format("rtmLock.SetLock Status.ErrorCode:{0} ", result.Status.ErrorCode), Message.MessageType.Info);
                }
                else
                {
                    string info = string.Format("rtmLock.SetLock Response , channelName:{0}, channelType:{1}, lockName:{2} errorCode:{3}",
                        result.Response.ChannelName, result.Response.ChannelType, result.Response.LockName, result.Response.ErrorCode);
                    messageDisplay.AddMessage(info, Message.MessageType.Info);
                }
            }
        }

        public async void GetLock()
        {
            if (rtmClient != null)
            {
                IRtmLock rtmLock = rtmClient.GetLock();
                string channelName = this.LockChannelNameInput.text;
                string lockName = this.LockNameInput.text;

                var result = await rtmLock.GetLocksAsync(channelName, RTM_CHANNEL_TYPE.RTM_CHANNEL_TYPE_MESSAGE);

                if (result.Status.Error)
                {
                    messageDisplay.AddMessage(string.Format("rtmLock.GetLocks Status.ErrorCode:{0} ", result.Status.ErrorCode), Message.MessageType.Info);
                }
                else
                {
                    string info = string.Format("rtmLock.GetLocks Response: ,channelName:{0},channelType:{1},count:{2},errorCode:{3},",
                        result.Response.ChannelName, result.Response.ChannelType, result.Response.Count, result.Response.ErrorCode);
                    messageDisplay.AddMessage(info, Message.MessageType.Info);
                    if (result.Response.Count > 0)
                    {
                        for (int i = 0; i < result.Response.LockDetailList.Length; i++)
                        {
                            var detail = result.Response.LockDetailList[i];
                            string info2 = string.Format("lockDetailList lockName:{0}, owner:{1}, ttl:{2}",
                                detail.lockName, detail.owner, detail.ttl);
                            messageDisplay.AddMessage(info2, Message.MessageType.Info);
                        }
                    }
                }
            }
        }

        public async void RemoveLock()
        {
            if (rtmClient != null)
            {
                IRtmLock rtmLock = rtmClient.GetLock();
                string channelName = this.LockChannelNameInput.text;
                string lockName = this.LockNameInput.text;

                var result = await rtmLock.RemoveLockAsync(channelName, RTM_CHANNEL_TYPE.RTM_CHANNEL_TYPE_MESSAGE, lockName);

                if (result.Status.Error)
                {
                    messageDisplay.AddMessage(string.Format("rtmLock.RemoveLock Status.ErrorCode:{0} ", result.Status.ErrorCode), Message.MessageType.Info);
                }
                else
                {
                    string info = string.Format("rtmLock.RemoveLock Response ,channelName:{0},channelType:{1},lockName:{2},errorCode:{3}",
                        result.Response.ChannelName, result.Response.ChannelType, result.Response.LockName, result.Response.ErrorCode);
                    messageDisplay.AddMessage(info, Message.MessageType.Info);
                }
            }
        }

        public async void AcquireLock()
        {
            if (rtmClient != null)
            {
                IRtmLock rtmLock = rtmClient.GetLock();
                string channelName = this.LockChannelNameInput.text;
                string lockName = this.LockNameInput.text;

                var result = await rtmLock.AcquireLockAsync(channelName, RTM_CHANNEL_TYPE.RTM_CHANNEL_TYPE_MESSAGE, lockName, true);

                if (result.Status.Error)
                {
                    messageDisplay.AddMessage(string.Format("rtmLock.AcquireLock Status.ErrorCode:{0} ", result.Status.ErrorCode), Message.MessageType.Info);
                }
                else
                {
                    string info = string.Format("rtmLock.AcquireLock Response ,channelName:{0},channelType:{1},lockName:{2},errorCode:{3}",
                        result.Response.ChannelName, result.Response.ChannelType, result.Response.LockName, result.Response.ErrorCode);
                    messageDisplay.AddMessage(info, Message.MessageType.Info);
                }
            }
        }

        public async void ReleaseLock()
        {
            if (rtmClient != null)
            {
                IRtmLock rtmLock = rtmClient.GetLock();
                string channelName = this.LockChannelNameInput.text;
                string lockName = this.LockNameInput.text;

                var result = await rtmLock.ReleaseLockAsync(channelName, RTM_CHANNEL_TYPE.RTM_CHANNEL_TYPE_MESSAGE, lockName);

                if (result.Status.Error)
                {
                    messageDisplay.AddMessage(string.Format("rtmLock.ReleaseLock Status.ErrorCode:{0} ", result.Status.ErrorCode), Message.MessageType.Info);
                }
                else
                {
                    string info = string.Format("rtmLock.ReleaseLock Response ,channelName:{0},channelType:{1},lockName:{2},errorCode:{3}",
                       result.Response.ChannelName, result.Response.ChannelType, result.Response.LockName, result.Response.ErrorCode);
                    messageDisplay.AddMessage(info, Message.MessageType.Info);
                }
            }
        }

        public async void RevokeLock()
        {
            if (rtmClient != null)
            {
                IRtmLock rtmLock = rtmClient.GetLock();
                string channelName = this.LockChannelNameInput.text;
                string lockName = this.LockNameInput.text;
                string owner = this.LockOwnerInput.text;

                var result = await rtmLock.RevokeLockAsync(channelName, RTM_CHANNEL_TYPE.RTM_CHANNEL_TYPE_MESSAGE, lockName, owner);
                if (result.Status.Error)
                {
                    messageDisplay.AddMessage(string.Format("rtmLock.RevokeLock Status.ErrorCode:{0} ", result.Status.ErrorCode), Message.MessageType.Info);
                }
                else
                {
                    string info = string.Format("rtmLock.RevokeLock Response ,channelName:{0},channelType:{1},lockName:{2},errorCode:{3}",
                        result.Response.ChannelName, result.Response.ChannelType, result.Response.LockName, result.Response.ErrorCode);
                    messageDisplay.AddMessage(info, Message.MessageType.Info);
                }
            }
        }

        #endregion

        #region Presence
        public async void WhoNow()
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

                var result = await rtmPresence.WhoNowAsync(channelName, RTM_CHANNEL_TYPE.RTM_CHANNEL_TYPE_MESSAGE, presenceOptions);
                if (result.Status.Error)
                {
                    messageDisplay.AddMessage(string.Format("rtmPresence.WhoNow Status.ErrorCode:{0} ", result.Status.ErrorCode), Message.MessageType.Info);
                }
                else
                {
                    string info = string.Format("rtmPresence.WhoNow Response ,count:{0},errorCode:{1},",
                        result.Response.Count, result.Response.ErrorCode);
                    messageDisplay.AddMessage(info, Message.MessageType.Info);
                    if (result.Response.Count > 0)
                    {
                        for (int i = 0; i < result.Response.UserStateList.Length; i++)
                        {
                            var userState = result.Response.UserStateList[i];
                            string info2 = string.Format("userStateList userId:{0}, stateCount:{1}", userState.userId, userState.statesCount);
                            messageDisplay.AddMessage(info2, Message.MessageType.Info);
                        }
                    }
                }
            }
        }

        public async void WhereNow()
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

                var result = await rtmPresence.WhereNowAsync(userId);
                if (result.Status.Error)
                {
                    messageDisplay.AddMessage(string.Format("rtmPresence.WhereNow Status.ErrorCode:{0} ", result.Status.ErrorCode), Message.MessageType.Info);
                }
                else
                {
                    string info = string.Format("rtmPresence.WhereNow Response: count:{1},errorCode:{2},"
                        , result.Response.Count, result.Response.ErrorCode);
                    messageDisplay.AddMessage(info, Message.MessageType.Info);
                    if (result.Response.Count > 0)
                    {
                        for (int i = 0; i < result.Response.Channels.Length; i++)
                        {
                            var channelInfo = result.Response.Channels[i];
                            string info2 = string.Format("userStateList channelName:{0}, channelType:{1}", channelInfo.channelName, channelInfo.channelType);
                            messageDisplay.AddMessage(info2, Message.MessageType.Info);
                        }
                    }
                }
            }
        }

        public async void SetState()
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

                var result = await rtmPresence.SetStateAsync(channelName, RTM_CHANNEL_TYPE.RTM_CHANNEL_TYPE_MESSAGE, stateItems);
                if (result.Status.Error)
                {
                    messageDisplay.AddMessage(string.Format("rtmPresence.SetState Status.ErrorCode:{0} ", result.Status.ErrorCode), Message.MessageType.Info);
                }
                else
                {
                    string info = string.Format("rtmPresence.SetState Response ,errorCode:{1},", result.Response.ErrorCode);
                    messageDisplay.AddMessage(info, Message.MessageType.Info);
                }
            }
        }

        public async void RemoveState()
        {
            if (rtmClient != null)
            {
                IRtmPresence rtmPresence = rtmClient.GetPresence();
                string channelName = this.PresenceChannelNameInput.text;
                string key = this.PresenceKeyInput.text;
                string value = this.PresenceValueInput.text;
                string userId = this.PresenceUserIdInput.text;
                string[] keys = new string[] { key };


                var result = await rtmPresence.RemoveStateAsync(channelName, RTM_CHANNEL_TYPE.RTM_CHANNEL_TYPE_MESSAGE, keys);
                if (result.Status.Error)
                {
                    messageDisplay.AddMessage(string.Format("rtmPresence.RemoveState Status.ErrorCode:{0} ", result.Status.ErrorCode), Message.MessageType.Info);
                }
                else
                {
                    string info = string.Format("rtmPresence.RemoveState Response errorCode:{0},", result.Response.ErrorCode);
                    messageDisplay.AddMessage(info, Message.MessageType.Info);
                }
            }
        }

        public async void GetState()
        {
            if (rtmClient != null)
            {
                IRtmPresence rtmPresence = rtmClient.GetPresence();
                string channelName = this.PresenceChannelNameInput.text;
                string key = this.PresenceKeyInput.text;
                string value = this.PresenceValueInput.text;
                string userId = this.PresenceUserIdInput.text;

                var result = await rtmPresence.GetStateAsync(channelName, RTM_CHANNEL_TYPE.RTM_CHANNEL_TYPE_MESSAGE, userId);
                if (result.Status.Error)
                {
                    messageDisplay.AddMessage(string.Format("rtmPresence.GetState Status.ErrorCode:{0} ", result.Status.ErrorCode), Message.MessageType.Info);
                }
                else
                {
                    string info = string.Format("rtmPresence.GetState Response ,errorCode:{0},", result.Response.ErrorCode);
                    messageDisplay.AddMessage(info, Message.MessageType.Info);

                    string info2 = string.Format("userStateList userId:{0}, stateCount:{1}",
                        result.Response.State.userId, result.Response.State.statesCount);
                    messageDisplay.AddMessage(info2, Message.MessageType.Info);
                }
            }
        }

        #endregion

        #region Storage

        public async void SetChannelMetadata()
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

                var result = await rtmStorage.SetChannelMetadataAsync(channelName, RTM_CHANNEL_TYPE.RTM_CHANNEL_TYPE_MESSAGE, GetRtmMetadata(), metadataOptions, lockName);
                if (result.Status.Error)
                {
                    messageDisplay.AddMessage(string.Format("rtmStorage.SetChannelMetadata Status.ErrorCode:{0} ", result.Status.ErrorCode), Message.MessageType.Info);
                }
                else
                {
                    string info = string.Format("rtmStorage.SetChannelMetadata Response ,channelName:{0},channelType:{1},errorCode:{2}",
                        result.Response.ChannelName, result.Response.ChannelType, result.Response.ErrorCode);
                    messageDisplay.AddMessage(info, Message.MessageType.Info);
                }
            }
        }

        public async void UpdateChannelMetadata()
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

                var result = await rtmStorage.UpdateChannelMetadataAsync(channelName, RTM_CHANNEL_TYPE.RTM_CHANNEL_TYPE_MESSAGE, GetRtmMetadata(), metadataOptions, lockName);
                if (result.Status.Error)
                {
                    messageDisplay.AddMessage(string.Format("rtmStorage.UpdateChannelMetadata Status.ErrorCode:{0} ", result.Status.ErrorCode), Message.MessageType.Info);
                }
                else
                {
                    string info = string.Format("rtmStorage.UpdateChannelMetadata Response,channelName:{0},channelType:{1},errorCode:{2}",
                        result.Response.ChannelName, result.Response.ChannelType, result.Response.ErrorCode);
                    messageDisplay.AddMessage(info, Message.MessageType.Info);
                }
            }
        }

        public async void RemoveChannelMetadata()
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

                var result = await rtmStorage.RemoveChannelMetadataAsync(channelName, RTM_CHANNEL_TYPE.RTM_CHANNEL_TYPE_MESSAGE, GetRtmMetadata(), metadataOptions, lockName);
                if (result.Status.Error)
                {
                    messageDisplay.AddMessage(string.Format("rtmStorage.RemoveChannelMetadata Status.ErrorCode:{0} ", result.Status.ErrorCode), Message.MessageType.Info);
                }
                else
                {
                    string info = string.Format("rtmStorage.RemoveChannelMetadata Response ,channelName:{0},channelType:{1},errorCode:{2}",
                        result.Response.ChannelName, result.Response.ChannelType, result.Response.ErrorCode);
                    messageDisplay.AddMessage(info, Message.MessageType.Info);
                }
            }
        }

        public async void GetChannelMetadata()
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

                var result = await rtmStorage.GetChannelMetadataAsync(channelName, RTM_CHANNEL_TYPE.RTM_CHANNEL_TYPE_MESSAGE);
                if (result.Status.Error)
                {
                    messageDisplay.AddMessage(string.Format("rtmStorage.GetChannelMetadata Status.ErrorCode:{0} ", result.Status.ErrorCode), Message.MessageType.Info);
                }
                else
                {
                    string info = string.Format("rtmStorage.GetChannelMetadata Response ,channelName:{0},channelType:{1},errorCode:{2}",
                        result.Response.ChannelName, result.Response.ChannelType, result.Response.ErrorCode);
                    messageDisplay.AddMessage(info, Message.MessageType.Info);
                }
            }
        }

        public async void SetUserMetadata()
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


                var result = await rtmStorage.SetUserMetadataAsync(userId, GetRtmMetadata(), metadataOptions);
                if (result.Status.Error)
                {
                    messageDisplay.AddMessage(string.Format("rtmStorage.SetUserMetadata Status.ErrorCode:{0} ", result.Status.ErrorCode), Message.MessageType.Info);
                }
                else
                {
                    string info = string.Format("rtmStorage.SetUserMetadata Response ,userId:{0},errorCode:{1}",
                        result.Response.UserId, result.Response.ErrorCode);
                    messageDisplay.AddMessage(info, Message.MessageType.Info);
                }
            }
        }

        public async void UpdateUserMetadata()
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

                var result = await rtmStorage.UpdateUserMetadataAsync(userId, GetRtmMetadata(), metadataOptions);
                if (result.Status.Error)
                {
                    messageDisplay.AddMessage(string.Format("rtmStorage.UpdateUserMetadata Status.ErrorCode:{0} ", result.Status.ErrorCode), Message.MessageType.Info);
                }
                else
                {
                    string info = string.Format("rtmStorage.UpdateUserMetadata Response ,userId:{1},errorCode:{2}",
                        result.Response.UserId, result.Response.ErrorCode);
                    messageDisplay.AddMessage(info, Message.MessageType.Info);
                }
            }
        }

        public async void RemoveUserMetadata()
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

                var result = await rtmStorage.RemoveUserMetadataAsync(userId, GetRtmMetadata(), metadataOptions);
                if (result.Status.Error)
                {
                    messageDisplay.AddMessage(string.Format("rtmStorage.RemoveUserMetadata Status.ErrorCode:{0} ", result.Status.ErrorCode), Message.MessageType.Info);
                }
                else
                {
                    string info = string.Format("rtmStorage.RemoveUserMetadata Response ,userId:{0},errorCode:{1}",
                        result.Response.UserId, result.Response.ErrorCode);
                    messageDisplay.AddMessage(info, Message.MessageType.Info);
                }
            }
        }

        public async void GetUserMetadata()
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


                var result = await rtmStorage.GetUserMetadataAsync(userId);
                if (result.Status.Error)
                {
                    messageDisplay.AddMessage(string.Format("rtmStorage.GetUserMetadata Status.ErrorCode:{0} ", result.Status.ErrorCode), Message.MessageType.Info);
                }
                else
                {
                    string info = string.Format("rtmStorage.GetUserMetadata Response ,userId:{0},errorCode:{1}",
                        result.Response.UserId, result.Response.ErrorCode);
                    messageDisplay.AddMessage(info, Message.MessageType.Info);
                    DisplayRtmMetadata(ref result.Response.Data);
                }
            }
        }

        public async void SubscribeUserMetadata()
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

                var result = await rtmStorage.SubscribeUserMetadataAsync(userId);
                if (result.Status.Error)
                {
                    messageDisplay.AddMessage(string.Format("rtmStorage.SubscribeUserMetadata Status.ErrorCode:{0} ", result.Status.ErrorCode), Message.MessageType.Info);
                }
                else
                {
                    string info = string.Format("rtmStorage.SubscribeUserMetadata Response userId:{0},errorCode:{1}",
                        result.Response.UserId, result.Response.ErrorCode);
                    messageDisplay.AddMessage(info, Message.MessageType.Info);
                }
            }
        }

        public async void UnsubscribeUserMetadata()
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

                var result = await rtmStorage.UnsubscribeUserMetadataAsync(userId);
                messageDisplay.AddMessage("IRtmStorage.UnsubscribeUserMetadata  ret:" + result.Status.ErrorCode, Message.MessageType.Info);
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


        #region callback


        public void OnMessageEvent(MessageEvent @event)
        {
            messageDisplay.AddMessage("OnMessageEvent channelName : " + @event.channelName + " channelTopic :" + @event.channelTopic + " channelType : " + @event.channelType + " publisher : " + @event.publisher + " message : " + @event.message, Message.MessageType.TopicMessage);
        }

        public void OnPresenceEvent(PresenceEvent @event)
        {
            string str = string.Format("OnPresenceEvent: type:{0} channelType:{1} channelName:{2} publisher:{3}", @event.type, @event.channelType, @event.channelName, @event.publisher);
            messageDisplay.AddMessage(str, Message.MessageType.Info);
        }

        public void OnStorageEvent(StorageEvent @event)
        {
            string str = string.Format("OnStorageEvent: channelType:{0} eventType:{1} target:{2}", @event.channelType, @event.eventType, @event.target);
            messageDisplay.AddMessage(str, Message.MessageType.Info);
            if (@event.data != null)
            {
                DisplayRtmMetadata(ref @event.data);
            }
        }

        public void OnTopicEvent(TopicEvent @event)
        {
            string str = string.Format("OnTopicEvent: channelName:{0} userId:{1}", @event.channelName, @event.userId);
            messageDisplay.AddMessage(str, Message.MessageType.Info);

            if (@event.topicInfoCount > 0)
            {
                for (ulong i = 0; i < @event.topicInfoCount; i++)
                {
                    var topicInfo = @event.topicInfos[i];
                    string str1 = string.Format("|--topicInfo {0}: topic:{1} publisherCount:{2}", i, topicInfo.topic, topicInfo.publisherCount);
                    messageDisplay.AddMessage(str1, Message.MessageType.Info);
                    if (topicInfo.publisherCount > 0)
                    {
                        for (ulong j = 0; j < topicInfo.publisherCount; j++)
                        {
                            var publisher = topicInfo.publishers[j];
                            string str2 = string.Format("  |--publisher {0}: userId:{1} meta:{2}", j, publisher.publisherUserId, publisher.publisherMeta);
                            messageDisplay.AddMessage(str2, Message.MessageType.Info);
                        }
                    }
                }
            }
        }

        public void OnLockEvent(LockEvent @event)
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

        public void OnConnectionStateChange(string channelName, RTM_CONNECTION_STATE state, RTM_CONNECTION_CHANGE_REASON reason)
        {
            string str1 = string.Format("OnConnectionStateChange channelName {0}: state:{1} reason:{2}", channelName, state, reason);
            messageDisplay.AddMessage(str1, Message.MessageType.Info);
        }

        public void OnTokenPrivilegeWillExpire(string channelName)
        {
            string str1 = string.Format("OnTokenPrivilegeWillExpire channelName {0}", channelName);
            messageDisplay.AddMessage(str1, Message.MessageType.Info);
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
}

