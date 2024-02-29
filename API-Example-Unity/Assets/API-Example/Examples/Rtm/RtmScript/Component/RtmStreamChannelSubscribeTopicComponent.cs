#define AGORA_RTC
#define AGORA_RTM

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Agora.Rtm;

namespace io.agora.rtm.demo
{
    public class RtmStreamChannelSubscribeTopicComponent : IRtmComponet
    {
        public InputField TopicInput;
        public ListContainerUser ContainerUser;


        public async void OnSubscribeTopic()
        {
            if (this.RtmScene.StreamChannel == null)
            {
                this.RtmScene.AddMessage("StreamChannel is null", Message.MessageType.Error);
                return;
            }

            if (this.TopicInput.text == "")
            {
                this.RtmScene.AddMessage("topic name is empty", Message.MessageType.Error);
                return;
            }

            TopicOptions options = new TopicOptions();
            options.users = this.ContainerUser.GetDataSource();

            this.RtmScene.AddMessage("Users:\n" + this.ContainerUser.ToString(), Message.MessageType.Info);

            var result = await this.RtmScene.StreamChannel.SubscribeTopicAsync(this.TopicInput.text, options);

            if (result.Status.Error)
            {
                this.RtmScene.AddMessage(string.Format("StreamChannel.SubscribeTopic Status.ErrorCode:{0} ", result.Status.ErrorCode), Message.MessageType.Info);
            }
            else
            {
                var succeedUsersCount = result.Response.SucceedUsers == null ? 0 : result.Response.SucceedUsers.Length;
                var FailedUsersCount = result.Response.FailedUsers == null ? 0 : result.Response.FailedUsers.Length;
                this.RtmScene.AddMessage("StreamChannel.SubscribeTopic Response:" + " channelName:" + result.Response.ChannelName + " userId :" + result.Response.UserId + " topic :" + result.Response.Topic + " succeedUsersCount :" + succeedUsersCount + " failedUsers :" + FailedUsersCount, Message.MessageType.Info);

                for (int i = 0; i < succeedUsersCount; i++)
                {
                    this.RtmScene.AddMessage("succeedUsers index " + i + " UserName is " + result.Response.SucceedUsers[i], Message.MessageType.Info);
                }

                for (int i = 0; i < FailedUsersCount; i++)
                {
                    this.RtmScene.AddMessage("failedUsers index " + i + " UserName is " + result.Response.FailedUsers[i], Message.MessageType.Info);
                }

                ContainerUser.ClearAllNode();
            }
        }

        public async void OnUnsubscribeTopic()
        {
            if (this.RtmScene.StreamChannel == null)
            {
                this.RtmScene.AddMessage("StreamChannel is null", Message.MessageType.Error);
                return;
            }

            if (this.TopicInput.text == "")
            {
                this.RtmScene.AddMessage("topic name is empty", Message.MessageType.Error);
                return;
            }

            TopicOptions options = new TopicOptions();
            options.users = this.ContainerUser.GetDataSource();

            var ret = await this.RtmScene.StreamChannel.UnsubscribeTopicAsync(this.TopicInput.text, options);
            this.RtmScene.AddMessage("StreamChannel.UnsubscribeTopic ret:" + ret, Message.MessageType.Info);
        }

        public async void OnGetSubscribedUserList()
        {
            if (this.RtmScene.StreamChannel == null)
            {
                this.RtmScene.AddMessage("StreamChannel is null", Message.MessageType.Error);
                return;
            }

            if (this.TopicInput.text == "")
            {
                this.RtmScene.AddMessage("topic name is empty", Message.MessageType.Error);
                return;
            }

            var result = await this.RtmScene.StreamChannel.GetSubscribedUserListAsync(this.TopicInput.text);
            if (result.Status.Error)
            {
                this.RtmScene.AddMessage("GetSubscribedTopic ErrorCode:" + result.Status.ErrorCode, Message.MessageType.Info);
            }
            else
            {
                var userList = result.Response.Users;
                var userCount = userList == null ? 0 : userList.Length;
                if (userCount == 0)
                {
                    this.RtmScene.AddMessage("GetSubscribedTopic return size is zero", Message.MessageType.Error);
                }
                else
                {
                    this.RtmScene.AddMessage("GetSubscribedTopic return size is " + userCount, Message.MessageType.Error);
                    foreach (var user in userList)
                    {
                        this.RtmScene.AddMessage("--- " + user, Message.MessageType.Info);
                    }
                }
            }
        }


    }
}
