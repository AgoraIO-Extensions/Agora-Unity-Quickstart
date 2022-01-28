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
        public bool enableFpa;
        public DownLoaderEnum(string URL, string PATH, bool enableFpa)
        {
            this.url = URL;
            this.path = PATH;
            this.enableFpa = enableFpa;
        }
    }
 
    public class HttpDownload
    {
        public ushort port { set; get; }

        public void HttpDownloader(object down)
        {
            HttpWebRequest request = null;
            HttpWebResponse response = null;
            DateTime timeBeforeDownload;
            string filename = Guid.NewGuid().ToString() + ".txt";

            try
            {
                timeBeforeDownload = DateTime.Now;
                
                Debug.Log("AgoraFpaUnityLog：下载准备");
                request = WebRequest.Create((down as DownLoaderEnum).url) as HttpWebRequest;

                if ((down as DownLoaderEnum).enableFpa)
                {
                    Debug.Log("AgoraFpaUnityLog：set proxy");
                    WebProxy proxyObject = new WebProxy("127.0.0.1", port);
                    request.Proxy = proxyObject;
                }

                //以下为接收响应的方法
                response = (HttpWebResponse) request.GetResponse();
                //创建接收流
                Stream stream = response.GetResponseStream();
                
                string dir = (down as DownLoaderEnum).path.Substring(0, (down as DownLoaderEnum).path.LastIndexOf("/"));
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                //文件写入路径
                FileStream file = new FileStream((down as DownLoaderEnum).path + filename, FileMode.OpenOrCreate, FileAccess.Write);
                //返回内容总长度
                int max = (int)response.ContentLength;
                int len = 0;
                while (len < max)
                {
                    //byte容器
                    byte[] data = new byte[10240000];
                    //循环读取
                    int _len = stream.Read(data, 0, data.Length);
                    //写入文件
                    file.Write(data, 0, _len);
                    len += _len;
                }

                file.Close();
                stream.Close();
                
                Debug.Log("AgoraFpaUnityLog: 下载完成, 用时：" + ((TimeSpan)(DateTime.Now - timeBeforeDownload)).TotalMilliseconds + "ms");
                File.Delete((down as DownLoaderEnum).path + filename);
            }
            catch (Exception ex)
            {
                Debug.LogError("AgoraFpaUnityLog: 错误==>>" + ex.Message);
            }
            finally
            {
                if (response != null)
                {
                    response.Close();
                }

                if (request != null)
                {
                    System.Threading.Thread.Sleep(2000);
                    request.Abort();
                }
            }
        }
    }
}