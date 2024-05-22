using System.Collections;
using System.Collections.Generic;
using Agora.Rtc;
using UnityEngine;
using UnityEngine.UI;

namespace Agora_RTC_Plugin.API_Example.Examples.Basic.JoinChannelVideo
{
    public class RemoteVideoCallQualityPanel : MonoBehaviour
    {
        public uint Uid = 0;
        public RemoteAudioStats AudioStats = null;
        public RemoteVideoStats VideoStats = null;

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
            if (AudioStats != null)
            {
                show.Add(string.Format("ARecv: {0}kbps", AudioStats.receivedBitrate));
                show.Add(string.Format("ALoss: {0}%", AudioStats.audioLossRate));
                show.Add(string.Format("AQuality: {0}", (QUALITY_TYPE)AudioStats.quality));
            }
            if (VideoStats != null)
            {
                show.Add(string.Format("{0}x{1},{2}fps", VideoStats.width, VideoStats.height, VideoStats.decoderOutputFrameRate));
                show.Add(string.Format("VRecv: {0}kbps", VideoStats.receivedBitrate));
                show.Add(string.Format("VLoss: {0}%", VideoStats.packetLossRate));
            }
            var text = this.GetComponentInChildren<Text>();
            text.text = string.Join("\n", show.ToArray());
        }
    }
}
