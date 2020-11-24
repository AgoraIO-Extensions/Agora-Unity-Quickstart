using UnityEngine;
using agora_gaming_rtc;

public class AudienceClientApp : PlayerViewControllerBase
{

    protected override void PrepareToJoin()
    {
        Debug.Log("AudienceClientApp prepare to join channel.");
        // Live Broadcasting mode to allow many view only audience 
        // IMPORTANT NOTE:  you can not leave the channel and join without specifying new profile
        //                  it will assume the same profile in the base class client.
        mRtcEngine.SetChannelProfile(CHANNEL_PROFILE.CHANNEL_PROFILE_LIVE_BROADCASTING);
        mRtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_AUDIENCE);
        base.PrepareToJoin();
    }
}
