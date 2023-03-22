using UnityEngine;
using System;
using UnityEngine.Serialization;

namespace Agora_RTC_Plugin.API_Example
{

    [CreateAssetMenu(menuName = "Agora/AppIdInput", fileName = "AppIdInput", order = 1)]
    [Serializable]
    public class AppIdInput : ScriptableObject
    {
        [FormerlySerializedAs("APP_ID")]
        [SerializeField]
        public string appID = "";

        [FormerlySerializedAs("TOKEN")]
        [SerializeField]
        public string token = "";

        [FormerlySerializedAs("CHANNEL_NAME")]
        [SerializeField]
        public string channelName = "YOUR_CHANNEL_NAME";
    }
}
