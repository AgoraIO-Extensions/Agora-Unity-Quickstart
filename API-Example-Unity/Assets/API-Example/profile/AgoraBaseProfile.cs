using UnityEngine;
using System.Collections;
using System;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Agora Profiles/Agora Basic Profile", fileName = "AgoraBaseProfile", order = 1)]
[Serializable]
public class AgoraBaseProfile : ScriptableObject
{
    [FormerlySerializedAs("APP_ID")] [SerializeField]
    public string appID = "";

    [FormerlySerializedAs("TOKEN")] [SerializeField]
    public string token = "";

    [FormerlySerializedAs("CHANNEL_NAME")] [SerializeField]
    public string channelName = "YOUR_CHANNEL_NAME";
}
