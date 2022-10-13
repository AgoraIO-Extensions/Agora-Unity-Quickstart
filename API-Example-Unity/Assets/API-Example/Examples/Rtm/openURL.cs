using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class openURL : MonoBehaviour
{

    [SerializeField]
    string URL = "";

    public void OnPress()
    {
        Application.OpenURL(URL);
    }
}
