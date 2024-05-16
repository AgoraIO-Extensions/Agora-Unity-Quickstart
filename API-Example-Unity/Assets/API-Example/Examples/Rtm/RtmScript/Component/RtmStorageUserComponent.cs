#define AGORA_RTC
#define AGORA_RTM

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Agora.Rtm;
namespace io.agora.rtm.demo
{
    public class RtmStorageUserComponent : IRtmComponet
    {
        public InputField UserIdInput;
        public Toggle RecordTsToggle;
        public Toggle RecordUserIdToggle;
        public InputField MajorRevisionInput;
        public ListContainerMetadataItem ContainerMetadataItem;


        public async void OnSetUserMetadata()
        {
            if (RtmScene.RtmClient == null)
            {
                RtmScene.AddMessage("RtmClient is null", Message.MessageType.Error);
                return;
            }

            if (this.UserIdInput.text == "")
            {
                RtmScene.AddMessage("User id is empty", Message.MessageType.Error);
                return;
            }

            if (this.MajorRevisionInput.text == "")
            {
                RtmScene.AddMessage("major revision name is empty", Message.MessageType.Error);
                return;
            }

            string userId = this.UserIdInput.text;
            MetadataOptions metadataOptions = new MetadataOptions()
            {
                recordUserId = RecordUserIdToggle.isOn,
                recordTs = RecordTsToggle.isOn
            };
            Agora.Rtm.Metadata rtmMetadata = new Agora.Rtm.Metadata();
            rtmMetadata.majorRevision = long.Parse(this.MajorRevisionInput.text);
            rtmMetadata.items = this.ContainerMetadataItem.GetDataSource();
            RtmScene.AddMessage("metadataItem List: \n" + this.ContainerMetadataItem.ToString(), Message.MessageType.Info);

            IRtmStorage rtmStorage = RtmScene.RtmClient.GetStorage();

            var result = await rtmStorage.SetUserMetadataAsync(userId, rtmMetadata, metadataOptions);
            if (result.Status.Error)
            {
                RtmScene.AddMessage(string.Format("SetUserMetadata Status.ErrorCode:{0} ", result.Status.ErrorCode), Message.MessageType.Error);
            }
            else
            {
                string info = string.Format("SetUserMetadata Response : userId:{0}",
                    result.Response.UserId);
                RtmScene.AddMessage(info, Message.MessageType.Info);
            }

        }

        public async void OnUpdateUserMetadata()
        {
            if (RtmScene.RtmClient == null)
            {
                RtmScene.AddMessage("RtmClient is null", Message.MessageType.Error);
                return;
            }

            if (this.UserIdInput.text == "")
            {
                RtmScene.AddMessage("User id is empty", Message.MessageType.Error);
                return;
            }

            if (this.MajorRevisionInput.text == "")
            {
                RtmScene.AddMessage("major revision name is empty", Message.MessageType.Error);
                return;
            }

            string userId = this.UserIdInput.text;
            MetadataOptions metadataOptions = new MetadataOptions()
            {
                recordUserId = RecordUserIdToggle.isOn,
                recordTs = RecordTsToggle.isOn
            };
            Agora.Rtm.Metadata rtmMetadata = new Agora.Rtm.Metadata();
            rtmMetadata.majorRevision = long.Parse(this.MajorRevisionInput.text);
            rtmMetadata.items = this.ContainerMetadataItem.GetDataSource();
            RtmScene.AddMessage("metadataItem List: \n" + this.ContainerMetadataItem.ToString(), Message.MessageType.Info);

            IRtmStorage rtmStorage = RtmScene.RtmClient.GetStorage();
            var result = await rtmStorage.UpdateUserMetadataAsync(userId, rtmMetadata, metadataOptions);
            if (result.Status.Error)
            {
                RtmScene.AddMessage(string.Format("UpdateUserMetadata Status.ErrorCode:{0} ", result.Status.ErrorCode), Message.MessageType.Info);
            }
            else
            {
                string info = string.Format("UpdateUserMetadata Response ,userId:{0}",
                    result.Response.UserId);
                RtmScene.AddMessage(info, Message.MessageType.Info);
            }

        }

        public async void OnRemoveUserMetadata()
        {
            if (RtmScene.RtmClient == null)
            {
                RtmScene.AddMessage("RtmClient is null", Message.MessageType.Error);
                return;
            }

            if (this.UserIdInput.text == "")
            {
                RtmScene.AddMessage("User id is empty", Message.MessageType.Error);
                return;
            }

            if (this.MajorRevisionInput.text == "")
            {
                RtmScene.AddMessage("major revision name is empty", Message.MessageType.Error);
                return;
            }

            string userId = this.UserIdInput.text;
            MetadataOptions metadataOptions = new MetadataOptions()
            {
                recordUserId = RecordUserIdToggle.isOn,
                recordTs = RecordTsToggle.isOn
            };
            Agora.Rtm.Metadata rtmMetadata = new Agora.Rtm.Metadata();
            rtmMetadata.majorRevision = long.Parse(this.MajorRevisionInput.text);
            rtmMetadata.items = this.ContainerMetadataItem.GetDataSource();
            RtmScene.AddMessage("metadataItem List: \n" + this.ContainerMetadataItem.ToString(), Message.MessageType.Info);

            IRtmStorage rtmStorage = RtmScene.RtmClient.GetStorage();
            var result = await rtmStorage.RemoveUserMetadataAsync(userId, rtmMetadata, metadataOptions);
            if (result.Status.Error)
            {
                RtmScene.AddMessage(string.Format("RemoveUserMetadata Status.ErrorCode:{0} ", result.Status.ErrorCode), Message.MessageType.Error);
            }
            else
            {
                string info = string.Format("RemoveUserMetadata Response ,userId:{0}",
                    result.Response.UserId);
                RtmScene.AddMessage(info, Message.MessageType.Info);
            }
        }

        public async void OnGetUserMetadata()
        {
            if (RtmScene.RtmClient == null)
            {
                RtmScene.AddMessage("RtmClient is null", Message.MessageType.Error);
                return;
            }

            if (this.UserIdInput.text == "")
            {
                RtmScene.AddMessage("User id is empty", Message.MessageType.Error);
                return;
            }

            string userId = this.UserIdInput.text;
            IRtmStorage rtmStorage = RtmScene.RtmClient.GetStorage();
            var result = await rtmStorage.GetUserMetadataAsync(userId);
            if (result.Status.Error)
            {
                RtmScene.AddMessage(string.Format("GetUserMetadata Status.ErrorCode:{0} ", result.Status.ErrorCode), Message.MessageType.Info);
            }
            else
            {
                string info = string.Format("GetUserMetadata Response ,userId:{0}",
                    result.Response.UserId);
                RtmScene.AddMessage(info, Message.MessageType.Info);
                DisplayRtmMetadata(result.Response.Data);
            }

        }

        public async void OnSubscribeUserMetadata()
        {
            if (RtmScene.RtmClient == null)
            {
                RtmScene.AddMessage("RtmClient is null", Message.MessageType.Error);
                return;
            }

            if (this.UserIdInput.text == "")
            {
                RtmScene.AddMessage("User id is empty", Message.MessageType.Error);
                return;
            }

            string userId = this.UserIdInput.text;
            IRtmStorage rtmStorage = RtmScene.RtmClient.GetStorage();
            var result = await rtmStorage.SubscribeUserMetadataAsync(userId);
            if (result.Status.Error)
            {
                RtmScene.AddMessage(string.Format("SubscribeUserMetadata Status.ErrorCode:{0} ", result.Status.ErrorCode), Message.MessageType.Error);
            }
            else
            {
                string info = string.Format("SubscribeUserMetadata Response userId:{0}",
                    result.Response.UserId);
                RtmScene.AddMessage(info, Message.MessageType.Info);
            }

        }

        public async void OnUnsubscribeUserMetadata()
        {
            if (RtmScene.RtmClient == null)
            {
                RtmScene.AddMessage("RtmClient is null", Message.MessageType.Error);
                return;
            }

            if (this.UserIdInput.text == "")
            {
                RtmScene.AddMessage("User id is empty", Message.MessageType.Error);
                return;
            }

            string userId = this.UserIdInput.text;
            IRtmStorage rtmStorage = RtmScene.RtmClient.GetStorage();
            var result = await rtmStorage.UnsubscribeUserMetadataAsync(userId);
            RtmScene.AddMessage("IRtmStorage.UnsubscribeUserMetadata  ret:" + result.Status.ErrorCode, Message.MessageType.Info);

        }

        private void DisplayRtmMetadata(Agora.Rtm.Metadata data)
        {
            RtmScene.AddMessage("RtmMetadata.majorRevision:" + data.majorRevision, Message.MessageType.Info);
            if (data.items.Length > 0)
            {
                foreach (var item in data.items)
                {
                    RtmScene.AddMessage(string.Format("---- key:{0},value:{1},authorUserId:{2},revision:{3},updateTs:{4}", item.key, item.value, item.authorUserId, item.revision, item.updateTs), Message.MessageType.Info);
                }
            }
        }


    }
}
