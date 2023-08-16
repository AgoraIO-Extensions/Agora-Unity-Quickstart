#define AGORA_RTC
#define AGORA_RTM

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Agora.Rtm;
using UnityEngine.UI;

namespace io.agora.rtm.demo
{
    public class RtmLockComponent : IRtmComponet
    {
        public InputField ChannelNameInput;
        public EnumDropDown ChannelTypeDropDown;
        public InputField LockInput;

        public InputField TTLInput;
        public Toggle RetryToggle;
        public InputField OwnerInput;


        private void Start()
        {
            this.ChannelTypeDropDown.Init<RTM_CHANNEL_TYPE>();
        }

        #region Lock
        public async void OnGetLock()
        {
            if (this.RtmScene.RtmClient == null)
            {
                this.RtmScene.AddMessage("Rtm client is null", Message.MessageType.Error);
                return;
            }

            if (this.ChannelNameInput.text == "")
            {
                this.RtmScene.AddMessage("Channel name is empty", Message.MessageType.Error);
                return;
            }

            IRtmLock rtmLock = RtmScene.RtmClient.GetLock();
            string channelName = this.ChannelNameInput.text;
            RTM_CHANNEL_TYPE channelType = (RTM_CHANNEL_TYPE)this.ChannelTypeDropDown.GetSelectValue();

            var result = await rtmLock.GetLocksAsync(channelName, channelType);

            if (result.Status.Error)
            {
                RtmScene.AddMessage(string.Format("GetLocks Status.ErrorCode:{0} ", result.Status.ErrorCode), Message.MessageType.Error);
            }
            else
            {
                var LockDetailListCount = result.Response.LockDetailList == null ? 0 : result.Response.LockDetailList.Length;
                string info = string.Format("GetLocks Response: channelName:{0},channelType:{1},count:{2}",
                    result.Response.ChannelName, result.Response.ChannelType, LockDetailListCount);
                RtmScene.AddMessage(info, Message.MessageType.Info);
                if (LockDetailListCount > 0)
                {
                    for (int i = 0; i < result.Response.LockDetailList.Length; i++)
                    {
                        var detail = result.Response.LockDetailList[i];
                        string info2 = string.Format("{0} lockName:{1}, owner:{2}, ttl:{3}",
                            i, detail.lockName, detail.owner, detail.ttl);
                        RtmScene.AddMessage(info2, Message.MessageType.Info);
                    }
                }
            }

        }

        public async void OnRemoveLock()
        {
            if (this.RtmScene.RtmClient == null)
            {
                this.RtmScene.AddMessage("Rtm client is null", Message.MessageType.Error);
                return;
            }

            if (this.ChannelNameInput.text == "")
            {
                this.RtmScene.AddMessage("Channel name is empty", Message.MessageType.Error);
                return;
            }

            if (this.LockInput.text == "")
            {
                this.RtmScene.AddMessage("Lock name is empty", Message.MessageType.Error);
                return;
            }

            IRtmLock rtmLock = RtmScene.RtmClient.GetLock();
            string channelName = this.ChannelNameInput.text;
            string lockName = this.LockInput.text;
            RTM_CHANNEL_TYPE channelType = (RTM_CHANNEL_TYPE)this.ChannelTypeDropDown.GetSelectValue();

            var result = await rtmLock.RemoveLockAsync(channelName, channelType, lockName);
            if (result.Status.Error)
            {
                RtmScene.AddMessage(string.Format("RemoveLock Status.ErrorCode:{0} ", result.Status.ErrorCode), Message.MessageType.Error);
            }
            else
            {
                string info = string.Format("RemoveLock Response channelName:{0},channelType:{1},lockName:{2}",
                    result.Response.ChannelName, result.Response.ChannelType, result.Response.LockName);
                RtmScene.AddMessage(info, Message.MessageType.Info);
            }
        }

        public async void OnReleaseLock()
        {
            if (this.RtmScene.RtmClient == null)
            {
                this.RtmScene.AddMessage("Rtm client is null", Message.MessageType.Error);
                return;
            }

            if (this.ChannelNameInput.text == "")
            {
                this.RtmScene.AddMessage("Channel name is empty", Message.MessageType.Error);
                return;
            }

            if (this.LockInput.text == "")
            {
                this.RtmScene.AddMessage("Lock name is empty", Message.MessageType.Error);
                return;
            }

            IRtmLock rtmLock = RtmScene.RtmClient.GetLock();
            string channelName = this.ChannelNameInput.text;
            string lockName = this.LockInput.text;
            RTM_CHANNEL_TYPE channelType = (RTM_CHANNEL_TYPE)this.ChannelTypeDropDown.GetSelectValue();


            var result = await rtmLock.ReleaseLockAsync(channelName, RTM_CHANNEL_TYPE.MESSAGE, lockName);

            if (result.Status.Error)
            {
                RtmScene.AddMessage(string.Format("ReleaseLock Status.ErrorCode:{0} ", result.Status.ErrorCode), Message.MessageType.Error);
            }
            else
            {
                string info = string.Format("ReleaseLock Response:channelName:{0},channelType:{1},lockName:{2}",
                   result.Response.ChannelName, result.Response.ChannelType, result.Response.LockName);
                RtmScene.AddMessage(info, Message.MessageType.Info);
            }

        }

        public async void OnSetLock()
        {

            if (this.RtmScene.RtmClient == null)
            {
                this.RtmScene.AddMessage("Rtm client is null", Message.MessageType.Error);
                return;
            }

            if (this.ChannelNameInput.text == "")
            {
                this.RtmScene.AddMessage("Channel name is empty", Message.MessageType.Error);
                return;
            }

            if (this.LockInput.text == "")
            {
                this.RtmScene.AddMessage("Lock name is empty", Message.MessageType.Error);
                return;
            }

            if (this.TTLInput.text == "")
            {
                this.RtmScene.AddMessage("TTL is empty", Message.MessageType.Error);
                return;
            }

            IRtmLock rtmLock = RtmScene.RtmClient.GetLock();
            string channelName = this.ChannelNameInput.text;
            string lockName = this.LockInput.text;
            RTM_CHANNEL_TYPE channelType = (RTM_CHANNEL_TYPE)this.ChannelTypeDropDown.GetSelectValue();
            int ttl = int.Parse(this.TTLInput.text);


            var result = await rtmLock.SetLockAsync(channelName, channelType, lockName, ttl);

            if (result.Status.Error)
            {
                RtmScene.AddMessage(string.Format("SetLock Status.ErrorCode:{0} ", result.Status.ErrorCode), Message.MessageType.Error);
            }
            else
            {
                string info = string.Format("SetLock Response :channelName:{0}, channelType:{1}, lockName:{2}",
                    result.Response.ChannelName, result.Response.ChannelType, result.Response.LockName);
                RtmScene.AddMessage(info, Message.MessageType.Info);
            }
        }

        public async void OnAcquireLock()
        {
            if (this.RtmScene.RtmClient == null)
            {
                this.RtmScene.AddMessage("Rtm client is null", Message.MessageType.Error);
                return;
            }

            if (this.ChannelNameInput.text == "")
            {
                this.RtmScene.AddMessage("Channel name is empty", Message.MessageType.Error);
                return;
            }

            if (this.LockInput.text == "")
            {
                this.RtmScene.AddMessage("Lock name is empty", Message.MessageType.Error);
                return;
            }


            IRtmLock rtmLock = RtmScene.RtmClient.GetLock();
            string channelName = this.ChannelNameInput.text;
            string lockName = this.LockInput.text;
            RTM_CHANNEL_TYPE channelType = (RTM_CHANNEL_TYPE)this.ChannelTypeDropDown.GetSelectValue();
            bool retry = this.RetryToggle.isOn;

            var result = await rtmLock.AcquireLockAsync(channelName, channelType, lockName, retry);

            if (result.Status.Error)
            {
                RtmScene.AddMessage(string.Format("AcquireLock Status.ErrorCode:{0} ", result.Status.ErrorCode), Message.MessageType.Info);
            }
            else
            {
                string info = string.Format("AcquireLock Response : channelName:{0},channelType:{1},lockName:{2}",
                    result.Response.ChannelName, result.Response.ChannelType, result.Response.LockName);
                RtmScene.AddMessage(info, Message.MessageType.Info);
            }
        }

        public async void OnRevokeLock()
        {
            if (this.RtmScene.RtmClient == null)
            {
                this.RtmScene.AddMessage("Rtm client is null", Message.MessageType.Error);
                return;
            }

            if (this.ChannelNameInput.text == "")
            {
                this.RtmScene.AddMessage("Channel name is empty", Message.MessageType.Error);
                return;
            }

            if (this.LockInput.text == "")
            {
                this.RtmScene.AddMessage("Lock name is empty", Message.MessageType.Error);
                return;
            }

            if (this.OwnerInput.text == "")
            {
                this.RtmScene.AddMessage("Owner is empty", Message.MessageType.Error);
                return;
            }

            IRtmLock rtmLock = RtmScene.RtmClient.GetLock();
            string channelName = this.ChannelNameInput.text;
            string lockName = this.LockInput.text;
            RTM_CHANNEL_TYPE channelType = (RTM_CHANNEL_TYPE)this.ChannelTypeDropDown.GetSelectValue();
            string owner = this.OwnerInput.text;



            var result = await rtmLock.RevokeLockAsync(channelName, channelType, lockName, owner);
            if (result.Status.Error)
            {
                RtmScene.AddMessage(string.Format("rtmLock.RevokeLock Status.ErrorCode:{0} ", result.Status.ErrorCode), Message.MessageType.Info);
            }
            else
            {
                string info = string.Format("RevokeLock Response : channelName:{0},channelType:{1},lockName:{2}",
                    result.Response.ChannelName, result.Response.ChannelType, result.Response.LockName);
                RtmScene.AddMessage(info, Message.MessageType.Info);
            }
        }
        #endregion

    }
}
