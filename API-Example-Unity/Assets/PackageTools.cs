using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Agora_RTC_Plugin.API_Example
{
    public class PackageTools : MonoBehaviour
    {
        [MenuItem("AgoraRtm/ReplaceGUIDs")]
        public static void ReplaceGUIDs()
        {
            string[] oldGuids = new string[] {
                "2ca9649af72c248bbab820748557049f",
                "45e755d1adb9144308cabade1acad67b"
            };

            foreach (var oldGuid in oldGuids)
            {
                var newGuid = GUID.Generate().ToString();
                ReplaceGUID(oldGuid, newGuid);
            }
            Debug.Log("AgoraRtm ReplaceGUIDs finish");
        }

        protected static void ReplaceGUID(string oldGuid, string newGuid)
        {
            string[] files = Directory.GetFiles(Application.dataPath, "*.*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                var content = File.ReadAllText(file);
                if (Regex.IsMatch(content, oldGuid))
                {
                    content = content.Replace(oldGuid, newGuid);
                    File.WriteAllText(file, content);
                }
            }
            AssetDatabase.Refresh();
        }
    }
}
