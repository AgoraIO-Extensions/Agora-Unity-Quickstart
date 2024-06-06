using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Agora.Rtc;
using UnityEngine.UI;

namespace Agora_RTC_Plugin.API_Example.Examples.Basic.JoinChannelVideo
{

    public class LocalVideoCallQualityPanel : MonoBehaviour
    {
        public RtcStats Stats = null;
        public LocalAudioStats AudioStats = null;
        public LocalVideoStats VideoStats = null;

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }


        public void RefreshPanel()
        {
            List<string> show = new List<string>();

            show.Add("Local Quality:\n");

            if (Stats != null)
            {
                show.Add(string.Format("LM Delay: {0}ms", Stats.lastmileDelay));
                show.Add(string.Format("CPU: {0}% / {1}%", Stats.cpuAppUsage, Stats.cpuAppUsage));
                show.Add(string.Format("Send Loss: {0}%", Stats.txPacketLossRate));
            }
            if (AudioStats != null)
            {
                show.Add(string.Format("ASend: {0}kbps", AudioStats.sentBitrate));
            }
            if (VideoStats != null)
            {
                show.Add(string.Format("{0}x{1},{2}fps", VideoStats.encodedFrameWidth, VideoStats.encodedFrameHeight, VideoStats.encoderOutputFrameRate));
                show.Add(string.Format("VSend: {0}kbps", VideoStats.sentBitrate));
            }
            var text = this.GetComponentInChildren<Text>();
            text.text = string.Join("\n", show.ToArray());
        }



    }
}
