#define AGORA_RTC
#define AGORA_RTM

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace io.agora.rtm.demo
{
    public class RtmClientParametersComponent : IRtmComponet
    {
        public InputField ParametersInput;
        public List<string> ParametersList = new List<string>();

        public void OnSetParameters()
        {
            if (RtmScene.RtmClient == null)
            {
                RtmScene.AddMessage("RtmClient not init!!!", Message.MessageType.Error);
                return;
            }

            string parameter = this.ParametersInput.text;
            if (parameter == "")
            {
                RtmScene.AddMessage("parameter is empty", Message.MessageType.Error);
                return;
            }

            var result = RtmScene.RtmClient.SetParameters(parameter);

            if (result.Error)
            {
                RtmScene.AddMessage("SetParameters failed: " + result.ErrorCode, Message.MessageType.Error);

            }
            else
            {
                RtmScene.AddMessage("SetParameters :" + result.ErrorCode, Message.MessageType.Info);
                ParametersList.Add(parameter);
            }
        }

        public void OnMark()
        {
            string show = "already set parameters size:" + ParametersList.Count + "\n";
            foreach (var parameter in ParametersList)
            {
                show += ("    " + parameter + "\n");
            }
            RtmScene.AddMessage(show, Message.MessageType.Info);
        }
    }
}
