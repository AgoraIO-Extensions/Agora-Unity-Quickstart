#define AGORA_RTC
#define AGORA_RTM

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Agora.Rtm;
namespace io.agora.rtm.demo
{
    public class RtmStorageChannelComponent : IRtmComponet
    {
        public InputField ChannelNameInput;
        public EnumDropDown ChannelTypeDropDown;
        public Toggle RecordTsToggle;
        public Toggle RecordUserIdToggle;
        public InputField LockInput;
        public InputField MajorRevisionInput;
        public ListContainerMetadataItem ContainerMetadataItem;

        void Start()
        {
            this.ChannelTypeDropDown.Init<RTM_CHANNEL_TYPE>();
        }


        public async void OnSetChannelMetadata()
        {
            if (RtmScene.RtmClient == null)
            {
                RtmScene.AddMessage("RtmClient is null", Message.MessageType.Error);
                return;
            }

            if (this.ChannelNameInput.text == "")
            {
                RtmScene.AddMessage("Channel name is empty", Message.MessageType.Error);
                return;
            }

            if (this.MajorRevisionInput.text == "")
            {
                RtmScene.AddMessage("major revision name is empty", Message.MessageType.Error);
                return;
            }


            string channelName = this.ChannelNameInput.text;
            RTM_CHANNEL_TYPE channelType = (RTM_CHANNEL_TYPE)this.ChannelTypeDropDown.GetSelectValue();
            string lockName = this.LockInput.text;
            MetadataOptions metadataOptions = new MetadataOptions()
            {
                recordUserId = RecordUserIdToggle.isOn,
                recordTs = RecordTsToggle.isOn
            };
            RtmMetadata rtmMetadata = new RtmMetadata();
            rtmMetadata.majorRevision = long.Parse(this.MajorRevisionInput.text);
            rtmMetadata.metadataItems = this.ContainerMetadataItem.GetDataSource();
            RtmScene.AddMessage("metadataItem List: " + this.ContainerMetadataItem.ToString(), Message.MessageType.Info);

            IRtmStorage rtmStorage = RtmScene.RtmClient.GetStorage();
            var (status, response) = await rtmStorage.SetChannelMetadataAsync(channelName, channelType, rtmMetadata, metadataOptions, lockName);
            if (status.Error)
            {
                RtmScene.AddMessage(string.Format("SetChannelMetadata Status.ErrorCode:{0} ", status.ErrorCode), Message.MessageType.Info);
            }
            else
            {
                string info = string.Format("SetChannelMetadata Response : channelName:{0}, channelType:{1}",
                    response.ChannelName, response.ChannelType);
                RtmScene.AddMessage(info, Message.MessageType.Info);
            }
        }


        public async void OnUpdateChannelMetadata()
        {
            if (RtmScene.RtmClient == null)
            {
                RtmScene.AddMessage("RtmClient is null", Message.MessageType.Error);
                return;
            }

            if (this.ChannelNameInput.text == "")
            {
                RtmScene.AddMessage("Channel name is empty", Message.MessageType.Error);
                return;
            }

            if (this.MajorRevisionInput.text == "")
            {
                RtmScene.AddMessage("major revision name is empty", Message.MessageType.Error);
                return;
            }


            string channelName = this.ChannelNameInput.text;
            RTM_CHANNEL_TYPE channelType = (RTM_CHANNEL_TYPE)this.ChannelTypeDropDown.GetSelectValue();
            string lockName = this.LockInput.text;
            MetadataOptions metadataOptions = new MetadataOptions()
            {
                recordUserId = RecordUserIdToggle.isOn,
                recordTs = RecordTsToggle.isOn
            };
            RtmMetadata rtmMetadata = new RtmMetadata();
            rtmMetadata.majorRevision = long.Parse(this.MajorRevisionInput.text);
            rtmMetadata.metadataItems = this.ContainerMetadataItem.GetDataSource();
            RtmScene.AddMessage("metadataItem List: " + this.ContainerMetadataItem.ToString(), Message.MessageType.Info);


            IRtmStorage rtmStorage = RtmScene.RtmClient.GetStorage();
            var (status, response) = await rtmStorage.UpdateChannelMetadataAsync(channelName, channelType, rtmMetadata, metadataOptions, lockName);
            if (status.Error)
            {
                RtmScene.AddMessage(string.Format("UpdateChannelMetadata Status.ErrorCode:{0} ", status.ErrorCode), Message.MessageType.Info);
            }
            else
            {
                string info = string.Format("UpdateChannelMetadata Response: channelName:{0}, channelType:{1}",
                    response.ChannelName, response.ChannelType);
                RtmScene.AddMessage(info, Message.MessageType.Info);
            }
        }

        public async void OnRemoveChannelMetadata()
        {
            if (RtmScene.RtmClient == null)
            {
                RtmScene.AddMessage("RtmClient is null", Message.MessageType.Error);
                return;
            }

            if (this.ChannelNameInput.text == "")
            {
                RtmScene.AddMessage("Channel name is empty", Message.MessageType.Error);
                return;
            }

            if (this.MajorRevisionInput.text == "")
            {
                RtmScene.AddMessage("major revision name is empty", Message.MessageType.Error);
                return;
            }


            string channelName = this.ChannelNameInput.text;
            RTM_CHANNEL_TYPE channelType = (RTM_CHANNEL_TYPE)this.ChannelTypeDropDown.GetSelectValue();
            string lockName = this.LockInput.text;
            MetadataOptions metadataOptions = new MetadataOptions()
            {
                recordUserId = RecordUserIdToggle.isOn,
                recordTs = RecordTsToggle.isOn
            };
            RtmMetadata rtmMetadata = new RtmMetadata();
            rtmMetadata.majorRevision = long.Parse(this.MajorRevisionInput.text);
            rtmMetadata.metadataItems = this.ContainerMetadataItem.GetDataSource();
            RtmScene.AddMessage("metadataItem List: \n" + this.ContainerMetadataItem.ToString(), Message.MessageType.Info);


            IRtmStorage rtmStorage = RtmScene.RtmClient.GetStorage();

            var (status, response) = await rtmStorage.RemoveChannelMetadataAsync(channelName, channelType, rtmMetadata, metadataOptions, lockName);
            if (status.Error)
            {
                RtmScene.AddMessage(string.Format("rtmStorage.RemoveChannelMetadata Status.ErrorCode:{0} ", status.ErrorCode), Message.MessageType.Info);
            }
            else
            {
                string info = string.Format("rtmStorage.RemoveChannelMetadata Response : channelName:{0}, channelType:{1}",
                    response.ChannelName, response.ChannelType);
                RtmScene.AddMessage(info, Message.MessageType.Info);
            }

        }

        public async void OnGetChannelMetadata()
        {
            if (RtmScene.RtmClient == null)
            {
                RtmScene.AddMessage("RtmClient is null", Message.MessageType.Error);
                return;
            }

            if (this.ChannelNameInput.text == "")
            {
                RtmScene.AddMessage("Channel name is empty", Message.MessageType.Error);
                return;
            }

            string channelName = this.ChannelNameInput.text;
            RTM_CHANNEL_TYPE channelType = (RTM_CHANNEL_TYPE)this.ChannelTypeDropDown.GetSelectValue();

            IRtmStorage rtmStorage = RtmScene.RtmClient.GetStorage();

            var (status, response) = await rtmStorage.GetChannelMetadataAsync(channelName, channelType);
            if (status.Error)
            {
                RtmScene.AddMessage(string.Format("rtmStorage.GetChannelMetadata Status.ErrorCode:{0} ", status.ErrorCode), Message.MessageType.Info);
            }
            else
            {
                string info = string.Format("rtmStorage.GetChannelMetadata Response ,channelName:{0},channelType:{1}",
                    response.ChannelName, response.ChannelType);
                RtmScene.AddMessage(info, Message.MessageType.Info);
                DisplayRtmMetadata(ref response.Data);
            }
        }

        private void DisplayRtmMetadata(ref RtmMetadata data)
        {
            RtmScene.AddMessage("RtmMetadata.majorRevision:" + data.majorRevision, Message.MessageType.Info);
            if (data.metadataItemsSize > 0)
            {
                foreach (var item in data.metadataItems)
                {
                    RtmScene.AddMessage(string.Format("---- key:{0},value:{1},authorUserId:{2},revision:{3},updateTs:{4}", item.key, item.value, item.authorUserId, item.revision, item.updateTs), Message.MessageType.Info);
                }
            }
        }

    }
}
