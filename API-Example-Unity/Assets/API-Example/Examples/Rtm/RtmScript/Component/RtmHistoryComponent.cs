
#define AGORA_RTM

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
#if AGORA_RTC
using Agora.Rtc;
#endif
using Agora.Rtm;

namespace io.agora.rtm.demo
{
    public class RtmHistoryComponent : IRtmComponet
    {
        public InputField ChannelNameInput;
        public EnumDropDown ChannelTypeDropDown;
        public InputField MessageCountInput;
        public InputField StartInput;
        public InputField EndInput;

        public void Start()
        {
            this.ChannelTypeDropDown.Init<RTM_CHANNEL_TYPE>();
        }

        public async void OnGetMessages()
        {
            if (RtmScene.RtmClient == null)
            {
                RtmScene.AddMessage("RtmClient not init!!!", Message.MessageType.Error);
                return;
            }

            string channelName = ChannelNameInput.text;
            if (channelName == "")
            {
                RtmScene.AddMessage("channelName is empty", Message.MessageType.Error);
                return;
            }

            RTM_CHANNEL_TYPE channelType = (RTM_CHANNEL_TYPE)this.ChannelTypeDropDown.GetSelectValue();

            ushort messageCount = 0;
            try
            {
                messageCount = ushort.Parse(MessageCountInput.text);
            }
            catch (Exception e)
            {
                RtmScene.AddMessage("invalid messageCount", Message.MessageType.Error);
                return;
            }


            UInt64 start = 0;
            try
            {
                start = UInt64.Parse(StartInput.text);
            }
            catch (Exception e)
            {
                RtmScene.AddMessage("invalid start", Message.MessageType.Error);
                return;
            }

            UInt64 end = 0;
            try
            {
                end = UInt64.Parse(EndInput.text);
            }
            catch (Exception e)
            {
                RtmScene.AddMessage("invalid end", Message.MessageType.Error);
                return;
            }

            GetHistoryMessagesOptions options = new GetHistoryMessagesOptions();
            options.messageCount = messageCount;
            options.start = start;
            options.end = end;

            IRtmHistory rtmHistory = RtmScene.RtmClient.GetHistory();

            var result = await rtmHistory.GetMessages(channelName, channelType, options);

            if (result.Status.Error)
            {
                RtmScene.AddMessage(string.Format("rtmHistory.GetMessages Status.ErrorCode:{0} ", result.Status.ErrorCode), Message.MessageType.Error);
            }
            else
            {
                string info = string.Format("rtmClient.Publish Response");
                GetHistoryMessagesResult response = result.Response;
                RtmScene.AddMessage(string.Format("newStart: {0}, messageCount: {1}", response.NewStart, response.MessageList.Length), Message.MessageType.Info);
                foreach (var history in response.MessageList)
                {
                    RtmScene.AddMessage(string.Format("----messageType: {0},\npublisher: {1},\nmessage: {2},\ncustomType: {3},\ntimestamp: {4}",
                        history.messageType, history.publisher, history.message.GetData<string>(), history.customType, history.timestamp),
                        Message.MessageType.Info);
                }
            }

        }
    }
}
