using UnityEngine;
using UnityEngine.UI;

namespace Agora.Util
{
    public class Logger
    {
        Text text;

        public Logger(Text text)
        {
            this.text = text;
        }

        public void UpdateLog(string logMessage)
        {
            Debug.Log(logMessage);
            string srcLogMessage = text.text;
            if (srcLogMessage.Length > 400)
            {
                srcLogMessage = srcLogMessage.Substring(srcLogMessage.Length - 50);
            }

            srcLogMessage += "\r\n \r\n";
            srcLogMessage += logMessage;
            text.text = srcLogMessage;
        }

        public bool DebugAssert(bool condition, string message)
        {
            if (!condition)
            {
                UpdateLog(message);
                return false;
            }

            Debug.Assert(condition, message);
            return true;
        }
    }
}