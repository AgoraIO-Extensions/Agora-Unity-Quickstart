using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CDNManager : MonoBehaviour
{
    [SerializeField]
    List<string> CDNControlRoomURL;

    public void HandleCDNButtonTap(int number)
    {
        if (number > CDNControlRoomURL.Count - 1)
        {
            return;
        }

        HandleTwitchLink(number);
        if (!_taking_input)
        {
            Application.OpenURL(CDNControlRoomURL[number]);
        }
    }

    // The following logic just to set up the direct link to your twitch account's live control room
    bool _taking_input = false;
    string _input_string = "";
    int _twitchingNumber = -1;
    private void OnGUI()
    {
        if (_taking_input)
        {
            float xp = Screen.width - 200;
            float yp = Screen.height / 2;

            GUI.Label(new Rect(xp, yp - 20, 200, 30), "Please enter your Twitch user ID:");
            _input_string = GUI.TextField(new Rect(xp, yp, 200, 20), _input_string, 25);
            if (GUI.Button(new Rect(Screen.width - 100, yp + 25, 60, 30), "OK"))
            {
                _taking_input = false;
                if (!string.IsNullOrEmpty(_input_string))
                {
                    CDNControlRoomURL[_twitchingNumber] = CDNControlRoomURL[_twitchingNumber].Replace("<YOUR_ID>", _input_string);
                }
            }
        }
    }

    void HandleTwitchLink(int number)
    {
        string url = CDNControlRoomURL[number];
        // in case you didn't change the place holder
        if (url.Contains("twitch.tv") && url.Contains("<YOUR_ID>"))
        {
            _taking_input = true;
            _twitchingNumber = number;
        }
    }
}
