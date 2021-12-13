using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using agora.fpa;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;

namespace agora.util
{
    public class DownLoaderEnum
    {
        public string url;
        public string path;
        public DownLoaderEnum(string URL, string PATH)
        {
            url = URL;
            path = PATH;
        }
    }
 
    public class HttpDownload
    {
        public ushort port { set; get; }

        public void HttpDownloader(object down)
        {
            // if (!Directory.Exists((down as DownLoaderEnum).path))
            //     Directory.CreateDirectory((down as DownLoaderEnum).path);
            // string tempPath = System.IO.Path.GetDirectoryName((down as DownLoaderEnum).path) + @"\temp";
            // Debug.Log("AgoraFpaUnityLog：tempPath" + tempPath);
            // System.IO.Directory.CreateDirectory(tempPath);
            // string tempFile = tempPath + @"\" + System.IO.Path.GetFileName((down as DownLoaderEnum).path) + ".temp";
     
            // if (System.IO.File.Exists(tempFile))
            // {
            //     System.IO.File.Delete(tempFile);
            // }
            try
            {
                Debug.Log("AgoraFpaUnityLog：下载准备");
                WebProxy proxyObject = new WebProxy("127.0.0.1", port);
                //FileStream fs = new FileStream(tempFile, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                // 设置参数
                HttpWebRequest request = WebRequest.Create((down as DownLoaderEnum).url) as HttpWebRequest;
                request.Proxy = proxyObject;
                //发送请求并获取相应回应数据
                HttpWebResponse response = request.BeginGetResponse(null, null) as HttpWebResponse;
                response.Close();
                
                
                //直到request.GetResponse()程序才开始向目标网页发送Post请求
               // Stream responseStream = response.GetResponseStream();
                //创建本地文件写入流
                //Stream stream = new FileStream(tempFile, FileMode.Create);
               //  byte[] bArr = new byte[1024];
               //  int size = responseStream.Read(bArr, 0, (int)bArr.Length);
               //  Debug.Log("size=" + size);
               //  while (size > 0)
               //  {
               //      //stream.Write(bArr, 0, size);
               //      fs.Write(bArr, 0, size);
               //      size = responseStream.Read(bArr, 0, (int)bArr.Length);
               //  }
               //  //stream.Close();
               //  fs.Close();
               //  responseStream.Close();
               //  string suffixName = (down as DownLoaderEnum).url;
               //  int su = suffixName.LastIndexOf('/');
               //  suffixName = (down as DownLoaderEnum).path+suffixName.Substring(su);
               // // Debug.LogError(suffixName);
               //  System.IO.File.Move(tempFile, suffixName);
                // return true;
                Debug.Log("AgoraFpaUnityLog: 下载完成");
            }
            catch (Exception ex)
            {
                Debug.LogError("AgoraFpaUnityLog: 错误==>>" + ex.Message);
                //return false;
            }
        }
    }
}