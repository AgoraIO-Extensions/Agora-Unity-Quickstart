using UnityEngine;
using agora_gaming_rtc;

public class One2OneApp : PlayerViewControllerBase
{

    protected override void PrepareToJoin()
    {
        Debug.Log("AudienceClientApp prepare to join channel.");
        // Live Broadcasting mode to allow many view only audience 
        mRtcEngine.SetChannelProfile(CHANNEL_PROFILE.CHANNEL_PROFILE_COMMUNICATION);
        base.PrepareToJoin();
    }
}
