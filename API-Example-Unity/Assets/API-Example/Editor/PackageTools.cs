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
        public static void ReplaceGUIDs()
        {
            string[] oldGuids = new string[] {
                "c7b548af9d337405f889b92c979c9e36",
                "afbde366e660d4272b8d45e2d7d96f50",
                "300c6525f002a4dbaac41a5c4b054e35",
                "310468f085ef24732beac714c9bb64fd",
                "2496050ad79454c69b7285bad8bdc7d5"
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
                if (file.EndsWith(".cs"))
                    continue;
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
