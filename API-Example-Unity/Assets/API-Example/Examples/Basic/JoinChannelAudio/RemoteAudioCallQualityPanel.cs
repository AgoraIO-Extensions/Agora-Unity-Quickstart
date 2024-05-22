using System.Collections;
using System.Collections.Generic;
using Agora.Rtc;
using UnityEngine;
using UnityEngine.UI;

namespace Agora_RTC_Plugin.API_Example.Examples.Basic.JoinChannelAudio
{
    public class RemoteAudioCallQualityPanel : MonoBehaviour
    {

        public uint Uid = 0;
        public int Volume = -1;
        public RemoteAudioStats AudioStats = null;

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
            show.Add(string.Format("Remote Uid: {0}", Uid));
            if (Volume >= 0)
            {
                show.Add(string.Format("Volume: {0}", Volume));
            }
            if (AudioStats != null)
            {
                show.Add(string.Format("ARecv: {0}kbps", AudioStats.receivedBitrate));
                show.Add(string.Format("ALoss: {0}%", AudioStats.audioLossRate));
                show.Add(string.Format("AQuality: {0}", (QUALITY_TYPE)AudioStats.quality));
            }
            var text = this.GetComponentInChildren<Text>();
            text.text = string.Join("\n", show.ToArray());
        }
    }
}
