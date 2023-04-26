using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace io.agora.rtm.demo
{
    public class IRtmComponet : MonoBehaviour
    {
        public IRtmScene RtmScene = null;

        public void Init(IRtmScene rtmScene)
        {
            RtmScene = rtmScene;
        }

        public virtual void UpdateUI() {

        }
    }
}
