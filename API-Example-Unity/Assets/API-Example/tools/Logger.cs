using UnityEngine;
using UnityEngine.UI;

public class Logger
{
    Text text;

    public Logger(Text text)
    {
        this.text = text;
    }

    public void UpdateLog(string logMessage, bool error = false)
    {
        if (error)
        {
            Debug.LogError(logMessage);
        }
        else
        {
            Debug.Log(logMessage);
        }

        string srcLogMessage = text.text;
        if (srcLogMessage.Length > 1000)
        {
            srcLogMessage = "";
        }
        srcLogMessage += "\r\n \r\n";
        srcLogMessage += logMessage;
        text.text = srcLogMessage;
    }

    public bool DebugAssert(bool condition, string message)
    {
        if (!condition)
        {
            UpdateLog("<color=red>" + message + "</color>", error: true);
            return false;
        }
        Debug.Assert(condition, message);
        return true;
    }
}