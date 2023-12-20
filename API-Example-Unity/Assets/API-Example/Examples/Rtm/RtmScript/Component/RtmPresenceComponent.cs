#define AGORA_RTC
#define AGORA_RTM

using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Agora.Rtm;
namespace io.agora.rtm.demo
{
    public class RtmPresenceComponent : IRtmComponet
    {
        public InputField ChannelNameInput;
        public EnumDropDown ChannelTypeDropDown;
        public Toggle IncludeUserIdToggle;
        public Toggle IncludeStateToggle;
        public InputField PageInput;
        public InputField WhereNowUserIdInput;
        public ListContanierStateItem ContanierStateItem;
        public ListContainerUser ContainerKey;
        public InputField GetStateUserIdInput;

        void Start()
        {
            this.ChannelTypeDropDown.Init<RTM_CHANNEL_TYPE>();
        }

        #region Presence
        public async void OnWhoNow()
        {
            if (RtmScene.RtmClient == null)
            {
                RtmScene.AddMessage("Rtm client is null", Message.MessageType.Error);
                return;
            }

            if (this.ChannelNameInput.text == "")
            {
                RtmScene.AddMessage("channel name is empty", Message.MessageType.Error);
                return;
            }

            string channelName = this.ChannelNameInput.text;
            RTM_CHANNEL_TYPE channelType = (RTM_CHANNEL_TYPE)this.ChannelTypeDropDown.GetSelectValue();
            PresenceOptions options = new PresenceOptions()
            {
                includeUserId = this.IncludeUserIdToggle.isOn,
                includeState = this.IncludeStateToggle.isOn,
                page = this.PageInput.text
            };

            IRtmPresence rtmPresence = this.RtmScene.RtmClient.GetPresence();

            var result = await rtmPresence.WhoNowAsync(channelName, channelType, options);
            if (result.Status.Error)
            {
                RtmScene.AddMessage(string.Format("WhoNow Status.ErrorCode:{0} ", result.Status.ErrorCode), Message.MessageType.Error);
            }
            else
            {
                var count = result.Response.UserStateList.Length;
                string info = string.Format("WhoNow Response : count:{0},nextPage:{1}",
                    count, result.Response.NextPage);
                RtmScene.AddMessage(info, Message.MessageType.Info);
                if (count > 0)
                {
                    for (int i = 0; i < result.Response.UserStateList.Length; i++)
                    {
                        var userState = result.Response.UserStateList[i];
                        var statesCount = userState.states == null ? 0 : userState.states.Length;
                        string info2 = string.Format("userStateList userId:{0}, stateCount:{1}", userState.userId, statesCount);
                        RtmScene.AddMessage(info2, Message.MessageType.Info);
                    }
                }
            }
        }

        public async void OnGetOnlineUsersAsync() {
            if (RtmScene.RtmClient == null)
            {
                RtmScene.AddMessage("Rtm client is null", Message.MessageType.Error);
                return;
            }

            if (this.ChannelNameInput.text == "")
            {
                RtmScene.AddMessage("channel name is empty", Message.MessageType.Error);
                return;
            }

            string channelName = this.ChannelNameInput.text;
            RTM_CHANNEL_TYPE channelType = (RTM_CHANNEL_TYPE)this.ChannelTypeDropDown.GetSelectValue();
            GetOnlineUsersOptions options = new GetOnlineUsersOptions()
            {
                includeUserId = this.IncludeUserIdToggle.isOn,
                includeState = this.IncludeStateToggle.isOn,
                page = this.PageInput.text
            };

            IRtmPresence rtmPresence = this.RtmScene.RtmClient.GetPresence();

            var result = await rtmPresence.GetOnlineUsersAsync(channelName, channelType, options);
            if (result.Status.Error)
            {
                RtmScene.AddMessage(string.Format("GetOnlineUsers Status.ErrorCode:{0} ", result.Status.ErrorCode), Message.MessageType.Error);
            }
            else
            {
                var count = result.Response.UserStateList.Length;
                string info = string.Format("GetOnlineUsers Response : count:{0},nextPage:{1}",
                    count, result.Response.NextPage);
                RtmScene.AddMessage(info, Message.MessageType.Info);
                if (count > 0)
                {
                    for (int i = 0; i < result.Response.UserStateList.Length; i++)
                    {
                        var userState = result.Response.UserStateList[i];
                        var statesCount = userState.states == null ? 0 : userState.states.Length;
                        string info2 = string.Format("userStateList userId:{0}, stateCount:{1}", userState.userId, statesCount);
                        RtmScene.AddMessage(info2, Message.MessageType.Info);
                    }
                }
            }
        }


        public async void OnWhereNow()
        {

            if (RtmScene.RtmClient == null)
            {
                RtmScene.AddMessage("Rtm client is null", Message.MessageType.Error);
                return;
            }


            if (this.WhereNowUserIdInput.text == "")
            {
                RtmScene.AddMessage("Where now user id is empty", Message.MessageType.Error);
                return;
            }

            string userId = this.WhereNowUserIdInput.text;

            IRtmPresence rtmPresence = this.RtmScene.RtmClient.GetPresence();

            var result = await rtmPresence.WhereNowAsync(userId);
            if (result.Status.Error)
            {
                RtmScene.AddMessage(string.Format("WhereNow Status.ErrorCode:{0} ", result.Status.ErrorCode), Message.MessageType.Info);
            }
            else
            {
                var count = result.Response.Channels.Length;
                string info = string.Format("WhereNow Response: count:{0}"
                    , count);
                RtmScene.AddMessage(info, Message.MessageType.Info);
                if (count > 0)
                {
                    for (int i = 0; i < result.Response.Channels.Length; i++)
                    {
                        var channelInfo = result.Response.Channels[i];
                        string info2 = string.Format("---- channelName:{0}, channelType:{1}", channelInfo.channelName, channelInfo.channelType);
                        RtmScene.AddMessage(info2, Message.MessageType.Info);
                    }
                }
            }

        }

        public async void OnGetUserChannels() {
            if (RtmScene.RtmClient == null)
            {
                RtmScene.AddMessage("Rtm client is null", Message.MessageType.Error);
                return;
            }


            if (this.WhereNowUserIdInput.text == "")
            {
                RtmScene.AddMessage("Get user channel user id is empty", Message.MessageType.Error);
                return;
            }

            string userId = this.WhereNowUserIdInput.text;

            IRtmPresence rtmPresence = this.RtmScene.RtmClient.GetPresence();

            var result = await rtmPresence.GetUserChannelsAsync(userId);
            if (result.Status.Error)
            {
                RtmScene.AddMessage(string.Format("GetUserChannel Status.ErrorCode:{0} ", result.Status.ErrorCode), Message.MessageType.Info);
            }
            else
            {
                var count = result.Response.Channels.Length;
                string info = string.Format("GetUserChannel Response: count:{0}"
                    , count);
                RtmScene.AddMessage(info, Message.MessageType.Info);
                if (count > 0)
                {
                    for (int i = 0; i < result.Response.Channels.Length; i++)
                    {
                        var channelInfo = result.Response.Channels[i];
                        string info2 = string.Format("---- channelName:{0}, channelType:{1}", channelInfo.channelName, channelInfo.channelType);
                        RtmScene.AddMessage(info2, Message.MessageType.Info);
                    }
                }
            }
        }

        public async void OnSetState()
        {

            if (RtmScene.RtmClient == null)
            {
                RtmScene.AddMessage("Rtm client is null", Message.MessageType.Error);
                return;
            }

            if (this.ChannelNameInput.text == "")
            {
                RtmScene.AddMessage("channel name is empty", Message.MessageType.Error);
                return;
            }

            string channelName = this.ChannelNameInput.text;
            RTM_CHANNEL_TYPE channelType = (RTM_CHANNEL_TYPE)this.ChannelTypeDropDown.GetSelectValue();
            StateItem[] stateItems = this.ContanierStateItem.GetDataSource();

            RtmScene.AddMessage("stateItem List: \n" + this.ContanierStateItem.ToString(), Message.MessageType.Info);

            IRtmPresence rtmPresence = this.RtmScene.RtmClient.GetPresence();
            var result = await rtmPresence.SetStateAsync(channelName, channelType, stateItems);
            if (result.Status.Error)
            {
                RtmScene.AddMessage(string.Format("SetState Status.ErrorCode:{0} ", result.Status.ErrorCode), Message.MessageType.Error);
            }
            else
            {
                string info = string.Format("SetState Response");
                RtmScene.AddMessage(info, Message.MessageType.Info);
            }
        }

        public async void OnRemoveState()
        {
            if (RtmScene.RtmClient == null)
            {
                RtmScene.AddMessage("Rtm client is null", Message.MessageType.Error);
                return;
            }

            if (this.ChannelNameInput.text == "")
            {
                RtmScene.AddMessage("channel name is empty", Message.MessageType.Error);
                return;
            }

            string channelName = this.ChannelNameInput.text;
            RTM_CHANNEL_TYPE channelType = (RTM_CHANNEL_TYPE)this.ChannelTypeDropDown.GetSelectValue();
            string[] keys = this.ContainerKey.GetDataSource();

            RtmScene.AddMessage("key List: \n" + this.ContainerKey.ToString(), Message.MessageType.Info);

            IRtmPresence rtmPresence = this.RtmScene.RtmClient.GetPresence();

            var result = await rtmPresence.RemoveStateAsync(channelName, channelType, keys);
            if (result.Status.Error)
            {
                RtmScene.AddMessage(string.Format("RemoveState Status.ErrorCode:{0} ", result.Status.ErrorCode), Message.MessageType.Error);
            }
            else
            {
                string info = string.Format("RemoveState Response");
                RtmScene.AddMessage(info, Message.MessageType.Info);
            }

        }

        public async void OnGetState()
        {
            if (RtmScene.RtmClient == null)
            {
                RtmScene.AddMessage("Rtm client is null", Message.MessageType.Error);
                return;
            }

            if (this.ChannelNameInput.text == "")
            {
                RtmScene.AddMessage("channel name is empty", Message.MessageType.Error);
                return;
            }

            if (this.GetStateUserIdInput.text == "")
            {
                RtmScene.AddMessage("Get state userId is empty", Message.MessageType.Error);
                return;
            }

            string channelName = this.ChannelNameInput.text;
            RTM_CHANNEL_TYPE channelType = (RTM_CHANNEL_TYPE)this.ChannelTypeDropDown.GetSelectValue();
            string userId = this.GetStateUserIdInput.text;

            IRtmPresence rtmPresence = this.RtmScene.RtmClient.GetPresence();

            var result = await rtmPresence.GetStateAsync(channelName, channelType, userId);
            if (result.Status.Error)
            {
                RtmScene.AddMessage(string.Format("GetState Status.ErrorCode:{0} ", result.Status.ErrorCode), Message.MessageType.Error);
            }
            else
            {
                string info = string.Format("rtmPresence.GetState Response");
                RtmScene.AddMessage(info, Message.MessageType.Info);
                var statesCount = result.Response.State.states == null ? 0 : result.Response.State.states.Length;
                string info2 = string.Format("userStateList userId:{0}, stateCount:{1}",
                    result.Response.State.userId, statesCount);
                RtmScene.AddMessage(info2, Message.MessageType.Info);
                foreach (var stateItem in result.Response.State.states)
                {
                    string info3 = string.Format("key:{0},value:{1}", stateItem.key, stateItem.value);
                    RtmScene.AddMessage(info3, Message.MessageType.Info);
                }
            }
        }

        #endregion



    }
}
