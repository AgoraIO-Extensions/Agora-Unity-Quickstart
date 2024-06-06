using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Agora.Rtc;
using UnityEngine.UI;

namespace Agora_RTC_Plugin.API_Example.Examples.Basic.JoinChannelAudio
{

    public class LocalAudioCallQualityPanel : MonoBehaviour
    {
        public int Volume = -1;
        public RtcStats Stats = null;
        public LocalAudioStats AudioStats = null;

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
            if (Volume >= 0)
            {
                show.Add(string.Format("Volume: {0}", Volume));
            }
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
            var text = this.GetComponentInChildren<Text>();
            text.text = string.Join("\n", show.ToArray());
        }



    }
}
