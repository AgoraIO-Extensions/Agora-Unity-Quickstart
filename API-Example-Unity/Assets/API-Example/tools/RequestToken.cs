using System;
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class TokenObject {
  public string rtcToken;
}

namespace agora_utilities
{
  public static class HelperClass
  {
      public static IEnumerator FetchToken(string url, string channel, int userId, Action<string> callback = null) {
        UnityWebRequest www = UnityWebRequest.Get(string.Format("http://localhost:8080/rtc/{0}/publisher/uid/{1}/", channel, userId));
        yield return www.SendWebRequest();

        if(www.isNetworkError || www.isHttpError) {
            Debug.Log(www.error);
            callback(null);
            yield break;
        }

        TokenObject tokenInfo = JsonUtility.FromJson<TokenObject>(www.downloadHandler.text);

        callback(tokenInfo.rtcToken);
      }
  }
}
