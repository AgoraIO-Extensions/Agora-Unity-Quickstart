using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Net;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using agora.fpa;
using agora.util;
using Logger = agora.util.Logger;

public class FpaServiceSample : MonoBehaviour
{
    [FormerlySerializedAs("APP_ID")] [SerializeField]
    private string appID = "";

    [FormerlySerializedAs("TOKEN")] [SerializeField]
    private string token = "";

    public Text logText;
    public Text infoText;
    internal Logger Logger;
    internal Logger InfoLogger;
    private Button _downloadButton1; 
    private Button _downloadButton2;
    private Button _downloadButton3; 
    private Button _downloadButton4;
    private InputField field;
    private Toggle StartFpaToggle;

    internal IAgoraFpaProxyService _mFpaProxyService = null;
    
    private FpaChainInfo _fpaChainInfo1 = new FpaChainInfo("www.qq.com", 80, 259, true);
    private FpaChainInfo _fpaChainInfo2 = new FpaChainInfo("frank-web-demo.rtns.sd-rtn.com", 30113, 254, true);
    private FpaChainInfo _fpaChainInfo3 = new FpaChainInfo("148.153.93.30", 30103, 204, true);
    private FpaChainInfo _fpaChainInfo4 = new FpaChainInfo("164.52.28.236", 30102, 203, true);
    private FpaChainInfo _fpaChainInfo5 = new FpaChainInfo("164.52.28.236", 30102, 10086, false);
    private FpaChainInfo _fpaChainInfo6 = new FpaChainInfo("164.52.28.236", 30102, 10011, true);

    private const string DownlaodUrl1 = "https://frank-web-demo.rtns.sd-rtn.com:30113/1MB.txt";
    private const string DownlaodUrl2 = "http://148.153.93.30:30103/10MB.txt";
    private const string DownlaodUrl3 = "http://164.52.28.236:30102";
    private const string UplaodUrl1 = "https://frank-web-demo.rtns.sd-rtn.com:30113/upload";
    private const string UplaodUrl2 = "http://148.153.93.30:30103/upload";
    
    private const float Offset = 100;
    //proxy port
    ushort port = 0; 
    //transparent proxy port
    ushort proxy_port = 0;
    public static int download_state = 0;
    private int call_number = 1;
    private bool enableFpa = true;
    
    // Start is called before the first frame update
    void Start()
    {
        ServicePointManager.DefaultConnectionLimit = 512;
        ThreadPool.SetMinThreads(200, 200);
        SetUpUI();
        CheckAppId();
        InitFpaService();
        GetDiagnosisInfo();
        GetHttpProxyPort();
    }

    // Update is called once per frame
    void Update()
    {
        PermissionHelper.RequestReadWritePermission();
    }
    
    private void CheckAppId()
    {
        ThreadPool.SetMaxThreads(5, 5);
        Logger.DebugAssert(appID.Length > 10, "Please fill in your appId in VideoCanvas!!!!!");
    }

    private void SetUpUI()
    {
        Logger = new Logger(logText);
        InfoLogger = new Logger(infoText);
        _downloadButton1 = GameObject.Find("DownloadButton1").GetComponent<Button>();
        _downloadButton1.onClick.AddListener(onDownloadButton1Press);
        _downloadButton2 = GameObject.Find("DownloadButton2").GetComponent<Button>();
        _downloadButton2.onClick.AddListener(onDownloadButton2Press);
        _downloadButton3 = GameObject.Find("DownloadButton3").GetComponent<Button>();
        _downloadButton3.onClick.AddListener(onDownloadButton3Press);
        _downloadButton4 = GameObject.Find("DownloadButton4").GetComponent<Button>();
        _downloadButton4.onClick.AddListener(onDownloadButton4Press);

        field = GameObject.Find("InputField").GetComponent<InputField>();
        
        StartFpaToggle = GameObject.Find("StartFpaToggle").GetComponent<Toggle>();
        StartFpaToggle.onValueChanged.AddListener(onFpaToggle);
        StartFpaToggle.isOn = true;
    }

    private void InitFpaService()
    {
        //create AgoraFpaProxyService
        _mFpaProxyService = AgoraFpaProxyService.CreateAgoraFpaProxyService();
        if (_mFpaProxyService == null)
        {
            Debug.Log("AgoraFpaUnityLog: Failed to create AgoraFpaProxyService object!");
            return;
        }
        
        //set AgoraFpaProxyServiceEventHandler
        _mFpaProxyService.InitEventHandler(new UserEventHandler(this));

        StartFpaService();
    }

    private void StartFpaService()
    {
        InfoLogger.UpdateLog(string.Format("=========StartFpaService========="));
        
        //start FpaProxyService
        FpaProxyServiceConfig config = new FpaProxyServiceConfig();
        config.app_id = appID;
        config.token = token;
        var ret = _mFpaProxyService.Start(config);
        Debug.Log("AgoraFpaUnityLog: AgoraFpaProxyService.Start returns " + ret);
        InfoLogger.UpdateLog(string.Format("SDK Version: {0}", _mFpaProxyService.GetAgoraFpaProxyServiceSdkVersion()));
        InfoLogger.UpdateLog(string.Format("SDK Info: {0}", _mFpaProxyService.GetAgoraFpaProxyServiceSdkBuildInfo()));

        //set HttpProxyChainConfig
        FpaHttpProxyChainConfig http_config = new FpaHttpProxyChainConfig();
        http_config.chain_array = new FpaChainInfo[3]{_fpaChainInfo1, _fpaChainInfo2, _fpaChainInfo3};
        http_config.chain_array_size = 3;
        ret = _mFpaProxyService.SetOrUpdateHttpProxyChainConfig(http_config);
        Debug.Log("AgoraFpaUnityLog: AgoraFpaProxyService.SetOrUpdateHttpProxyChainConfig returns: " + ret);
    }

    private void StopFpaService()
    {
        InfoLogger.UpdateLog(string.Format("=========StopFpaService========="));
        
        //stop FpaProxyService
        var ret = _mFpaProxyService.Stop();
        Debug.Log("AgoraFpaUnityLog: AgoraFpaProxyService.Stop returns " + ret);
    }
    
    private void GetHttpProxyPort()
    {
        _mFpaProxyService.GetHttpProxyPort(ref port);
        InfoLogger.UpdateLog(string.Format("AgoraFpaUnityLog: HttpProxyPort: {0}", port));
    }

    private void GetTransparentProxyPort(FpaChainInfo info)
    {
        var ret = _mFpaProxyService.GetTransparentProxyPort(ref proxy_port, info);
        Debug.Log("AgoraFpaUnityLog: AgoraFpaProxyService.GetTransparentProxyPort returns: " + ret);
    }

    private void GetDiagnosisInfo()
    {
        FpaProxyServiceDiagnosisInfo info = new FpaProxyServiceDiagnosisInfo();
        var ret = _mFpaProxyService.GetDiagnosisInfo(out info);
        Debug.Log("AgoraFpaUnityLog: AgoraFpaProxyService.GetDiagnosisInfo returns: " + ret);
        InfoLogger.UpdateLog(string.Format("DiagnosisInfo_install_id: {0}, DiagnosisInfo_instance_id: {1}", info.install_id, info.instance_id));
    }
    
    
    private void OnApplicationQuit()
    {
        Debug.Log("AgoraFpaUnityLog: OnApplicationQuit");
        if (_mFpaProxyService == null) return;
        StopFpaService();
        _mFpaProxyService.Dispose();
    }

    private void onDownloadButton1Press()
    {
        call_number = Convert.ToInt32(field.text);
        for (int i = 0; i < call_number; i++)
        {
            GetHttpProxyPort();
            DownloadFile(DownlaodUrl1, port);
        }
    }
    
    private void onDownloadButton2Press()
    {
        call_number = Convert.ToInt32(field.text);
        for (int i = 0; i < call_number; i++)
        {
            GetHttpProxyPort();
            DownloadFile(DownlaodUrl2, port);
        }
    }
    
    private void onDownloadButton3Press()
    {
        call_number = Convert.ToInt32(field.text);
        for (int i = 0; i < call_number; i++)
        {
            GetTransparentProxyPort(_fpaChainInfo4);
            DownloadFile(DownlaodUrl3, proxy_port);
        }
    }

    private void onDownloadButton4Press()
    {
        call_number = Convert.ToInt32(field.text);
        for (int i = 0; i < call_number; i++)
        {
            GetTransparentProxyPort(_fpaChainInfo5);
            DownloadFile(DownlaodUrl3, proxy_port);
        }
    }

    private void onFpaToggle(bool isStart)
    {
        enableFpa = isStart;
        if (_mFpaProxyService == null) return;
        if (isStart)
        {
            StartFpaService();
        }
        else
        {
            StopFpaService();
        }
    }

    public void onClearButtonPress()
    {
        Logger.ClearLog();
        InfoLogger.ClearLog();
    }
    

    private void DownloadFile(string url, ushort port)
    {
        DownLoaderEnum down = new DownLoaderEnum("", "", enableFpa);
#if UNITY_ANDROID
        down = new DownLoaderEnum(url, Application.persistentDataPath + "/test_fpa/", enableFpa);
#elif UNITY_IOS
        down = new DownLoaderEnum(url, Application.persistentDataPath + "/test_fpa/", enableFpa);
#endif
        HttpDownload download = new HttpDownload();
        download.port = port;
        ThreadPool.QueueUserWorkItem(download.HttpDownloader, down);
    }

    internal class UserEventHandler : IAgoraFpaProxyServiceEventHandler
    {
        private readonly FpaServiceSample _serviceSample;

        internal UserEventHandler(FpaServiceSample serviceSample)
        {
            _serviceSample = serviceSample;
        }

        public override void OnAccelerationSuccess(FpaProxyConnectionInfo info)
        {
            _serviceSample.Logger.UpdateLog("OnAccelerationSuccess");
        }

        public override void OnConnected(FpaProxyConnectionInfo info)
        {
            _serviceSample.Logger.UpdateLog("OnConnected");
        }

        public override void OnDisconnectedAndFallback(FpaProxyConnectionInfo info, FPA_FAILED_REASON_CODE reason)
        {
            _serviceSample.Logger.UpdateLog(string.Format("OnDisconnectedAndFallback, reason: {0}", reason));
        }

        public override void OnConnectionFailed(FpaProxyConnectionInfo info, FPA_FAILED_REASON_CODE reason)
        {
            _serviceSample.Logger.UpdateLog(string.Format("OnConnectionFailed, reason: {0}", reason));
        }
    }
}
