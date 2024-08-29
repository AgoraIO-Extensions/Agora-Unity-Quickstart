using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using Agora.Rtc;


using System.Collections.Generic;
using io.agora.rtc.demo;
using System.Runtime.InteropServices;

namespace Agora_RTC_Plugin.API_Example.Examples.Basic.RenderWithAlphaSample
{
    public class RenderWithAlphaSample : MonoBehaviour
    {
        [FormerlySerializedAs("appIdInput")]
        [SerializeField]
        private AppIdInput _appIdInput;

        [Header("_____________Basic Configuration_____________")]
        [FormerlySerializedAs("APP_ID")]
        [SerializeField]
        private string _appID = "";

        [FormerlySerializedAs("TOKEN")]
        [SerializeField]
        private string _token = "";

        [FormerlySerializedAs("CHANNEL_NAME")]
        [SerializeField]
        private string _channelName = "";

        public Text LogText;
        internal Logger Log;
        internal IRtcEngine RtcEngine = null;

        public Texture2D TextureWithAlphaBuffer;
        public Texture2D TextureWithAlphaStitchMode;
        private uint _videoTrackId = 0;
        private byte[] _rgbaData = null;
        private byte[] _alphaData = null;
        private int _textureWidth;
        private int _textureHeight;

        // Use this for initialization
        private void Start()
        {

            LoadAssetData();
            if (CheckAppId())
            {
                InitEngine();
            }
        }

        // Update is called once per frame
        private void Update()
        {
            PermissionHelper.RequestMicrophontPermission();
            PermissionHelper.RequestCameraPermission();
        }

        //Show data in AgoraBasicProfile
        [ContextMenu("ShowAgoraBasicProfileData")]
        private void LoadAssetData()
        {
            if (_appIdInput == null) return;
            _appID = _appIdInput.appID;
            _token = _appIdInput.token;
            _channelName = _appIdInput.channelName;
        }

        private bool CheckAppId()
        {
            Log = new Logger(LogText);
            return Log.DebugAssert(_appID.Length > 10, "Please fill in your appId in API-Example/profile/appIdInput.asset");
        }

        private void InitEngine()
        {
            RtcEngine = Agora.Rtc.RtcEngine.CreateAgoraRtcEngine();
            UserEventHandler handler = new UserEventHandler(this);
            RtcEngineContext context = new RtcEngineContext();
            context.appId = _appID;
            context.channelProfile = CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING;
            context.audioScenario = AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT;
            context.areaCode = AREA_CODE.AREA_CODE_GLOB;
            RtcEngine.Initialize(context);
            RtcEngine.InitEventHandler(handler);
            RtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
            RtcEngine.EnableAudio();
            RtcEngine.EnableVideo();
        }

        #region -- Button Events ---

        private void ConvertColor32(Color32[] colors, byte[] rgbaData, byte[] alphaData)
        {
            int i = 0;
            int l = 0;
            foreach (var color in colors)
            {
                rgbaData[i++] = color.r;
                rgbaData[i++] = color.g;
                rgbaData[i++] = color.b;
                rgbaData[i++] = color.a;

                alphaData[l++] = color.a;
            }

        }

        #region PushVideoFrameWithAlphaBuffer
        public void OnStartPushVideoFrameWithAlphaBuffer()
        {

            InitTextureDataWithAlphaBuffer();
            VideoEncoderConfiguration videoEncoderConfiguration = new VideoEncoderConfiguration();
            videoEncoderConfiguration.dimensions.width = _textureWidth;
            videoEncoderConfiguration.dimensions.height = _textureHeight;
            videoEncoderConfiguration.advanceOptions.encodeAlpha = true;
            RtcEngine.SetVideoEncoderConfiguration(videoEncoderConfiguration);

            _videoTrackId = RtcEngine.CreateCustomVideoTrack();
            var options = new ChannelMediaOptions();
            options.publishCameraTrack.SetValue(false);
            options.publishMicrophoneTrack.SetValue(true);
            options.publishCustomVideoTrack.SetValue(true);
            options.customVideoTrackId.SetValue(_videoTrackId);
            RtcEngine.JoinChannel(_token, _channelName, 0, options);

            InvokeRepeating("UpdateForPushVideoFrameWithAlphaBuffer", 0.1f, 0.1f);
        }

        public void OnStopPushVideoFrameWithAlphaBuffer()
        {
            CancelInvoke("UpdateForPushVideoFrameWithAlphaBuffer");
            RtcEngine.LeaveChannel();
            RtcEngine.DestroyCustomVideoTrack(_videoTrackId);
            _videoTrackId = 0;
        }


        public void InitTextureDataWithAlphaBuffer()
        {
            Color32[] color32s = TextureWithAlphaBuffer.GetPixels32(0);
            _rgbaData = new byte[color32s.Length * 4];
            _alphaData = new byte[color32s.Length];
            ConvertColor32(color32s, _rgbaData, _alphaData);
            _textureWidth = TextureWithAlphaBuffer.width;
            _textureHeight = TextureWithAlphaBuffer.height;
            this.Log.UpdateLog(string.Format("AlphaBuffer width: {0}, height: {1}", _textureWidth, _textureHeight));
        }

        public void UpdateForPushVideoFrameWithAlphaBuffer()
        {
            var timetick = System.DateTime.Now.Ticks / 10000;
            ExternalVideoFrame externalVideoFrame = new ExternalVideoFrame();
            externalVideoFrame.type = VIDEO_BUFFER_TYPE.VIDEO_BUFFER_RAW_DATA;
            externalVideoFrame.format = VIDEO_PIXEL_FORMAT.VIDEO_PIXEL_RGBA;
            externalVideoFrame.buffer = _rgbaData;
            externalVideoFrame.alphaStitchMode = ALPHA_STITCH_MODE.NO_ALPHA_STITCH;

            externalVideoFrame.alphaBuffer = null;
            externalVideoFrame.fillAlphaBuffer = true;
            //If the format is not RGBA or BGRA, you need set alphaBuffer value and make fillAlphaBuffer false,
            //externalVideoFrame.alphaBuffer = _alphaData;
            //externalVideoFrame.fillAlphaBuffer = false;

            externalVideoFrame.stride = _textureWidth;
            externalVideoFrame.height = _textureHeight;
            externalVideoFrame.rotation = 180;
            externalVideoFrame.timestamp = timetick;
            var ret = RtcEngine.PushVideoFrame(externalVideoFrame, _videoTrackId);
            Debug.Log("PushVideoFrame ret = " + ret + " time: " + timetick);

        }
        #endregion

        #region PushVideoFrameWithAlphaStitchMode
        public void OnStartPushVideoFrameWithAlphaStitchMode()
        {
            InitTextureDataWithAlphaStitchMode();
            VideoEncoderConfiguration videoEncoderConfiguration = new VideoEncoderConfiguration();
            videoEncoderConfiguration.dimensions.width = _textureWidth;
            videoEncoderConfiguration.dimensions.height = _textureHeight / 2;
            videoEncoderConfiguration.advanceOptions.encodeAlpha = true;
            RtcEngine.SetVideoEncoderConfiguration(videoEncoderConfiguration);

            _videoTrackId = RtcEngine.CreateCustomVideoTrack();
            var options = new ChannelMediaOptions();
            options.publishCameraTrack.SetValue(false);
            options.publishMicrophoneTrack.SetValue(true);
            options.publishCustomVideoTrack.SetValue(true);
            options.customVideoTrackId.SetValue(_videoTrackId);
            RtcEngine.JoinChannel(_token, _channelName, 0, options);

            InvokeRepeating("UpdateForPushVideoFrameWithAlphaStitchMode", 0.1f, 0.1f);
        }

        public void OnStopPushVideoFrameWithAlphaStitchMode()
        {
            CancelInvoke("UpdateForPushVideoFrameWithAlphaStitchMode");
            RtcEngine.LeaveChannel();
            RtcEngine.DestroyCustomVideoTrack(_videoTrackId);
            _videoTrackId = 0;
        }

        public void InitTextureDataWithAlphaStitchMode()
        {
            Color32[] color32s = TextureWithAlphaBuffer.GetPixels32(0);
           
       
            var width = TextureWithAlphaBuffer.width;
            var height = TextureWithAlphaBuffer.height;
            TextureWithAlphaStitchMode = new Texture2D(width, height * 2, TextureFormat.RGBA32, false);
            Color32[] newColor32s = new Color32[color32s.Length * 2];
            //copy origin to top
            Array.Copy(color32s, newColor32s, color32s.Length);
            for (int i = color32s.Length; i < newColor32s.Length; i++)
            {
                newColor32s[i].r = color32s[i - color32s.Length].a;
                newColor32s[i].g = newColor32s[i].r;
                newColor32s[i].b = newColor32s[i].r;
                newColor32s[i].a = 255;
            }

            TextureWithAlphaStitchMode.SetPixels32(newColor32s);
            TextureWithAlphaStitchMode.Apply();
            _rgbaData = new byte[newColor32s.Length * 4];
            _alphaData = new byte[newColor32s.Length];
            ConvertColor32(newColor32s, _rgbaData, _alphaData);
            _textureWidth = TextureWithAlphaStitchMode.width;
            _textureHeight = TextureWithAlphaStitchMode.height;
            this.Log.UpdateLog(string.Format("AlphaStitchMode width: {0}, height: {1}", _textureWidth, _textureHeight));

            GameObject.Find("RawImage").GetComponent<RawImage>().texture = TextureWithAlphaStitchMode;
            GameObject.Find("RawImage").GetComponent<RectTransform>().sizeDelta = new Vector2(_textureWidth, _textureHeight);
        }

        public void UpdateForPushVideoFrameWithAlphaStitchMode()
        {
            var timetick = System.DateTime.Now.Ticks / 10000;
            ExternalVideoFrame externalVideoFrame = new ExternalVideoFrame();
            externalVideoFrame.type = VIDEO_BUFFER_TYPE.VIDEO_BUFFER_RAW_DATA;
            externalVideoFrame.format = VIDEO_PIXEL_FORMAT.VIDEO_PIXEL_RGBA;
            externalVideoFrame.buffer = _rgbaData;
            externalVideoFrame.alphaStitchMode = ALPHA_STITCH_MODE.ALPHA_STITCH_BELOW;

            externalVideoFrame.stride = _textureWidth;
            externalVideoFrame.height = _textureHeight;
            externalVideoFrame.rotation = 180;
            externalVideoFrame.timestamp = timetick;
            var ret = RtcEngine.PushVideoFrame(externalVideoFrame, _videoTrackId);
            Debug.Log("PushVideoFrame ret = " + ret + " time: " + timetick);

        }
        #endregion

        #region VirtualBackgroundWithAlpha

        public void OnStartVirtualBackgroundWithAlpha()
        {
            RtcEngine.SetLocalVideoDataSourcePosition(VIDEO_MODULE_POSITION.POSITION_POST_CAPTURER);

            var source = new VirtualBackgroundSource();
            source.background_source_type = BACKGROUND_SOURCE_TYPE.BACKGROUND_NONE;
            var segproperty = new SegmentationProperty();
            var nRet = RtcEngine.EnableVirtualBackground(true, source, segproperty, MEDIA_SOURCE_TYPE.PRIMARY_CAMERA_SOURCE);
            this.Log.UpdateLog("EnableVirtualBackground true :" + nRet);

            VideoEncoderConfiguration videoEncoderConfiguration = new VideoEncoderConfiguration();

            videoEncoderConfiguration.advanceOptions.encodeAlpha = true;
            RtcEngine.SetVideoEncoderConfiguration(videoEncoderConfiguration);

            RtcEngine.JoinChannel(_token, _channelName, "", 0);
        }


        public void OnStopVirtualBackgroundWithAlpha()
        {
            RtcEngine.SetLocalVideoDataSourcePosition(VIDEO_MODULE_POSITION.POSITION_PRE_ENCODER);

            var source = new VirtualBackgroundSource();
            source.background_source_type = BACKGROUND_SOURCE_TYPE.BACKGROUND_NONE;
            var segproperty = new SegmentationProperty();
            var nRet = RtcEngine.EnableVirtualBackground(false, source, segproperty, MEDIA_SOURCE_TYPE.PRIMARY_CAMERA_SOURCE);
            this.Log.UpdateLog("EnableVirtualBackground false :" + nRet);

            RtcEngine.LeaveChannel();
        }
        #endregion

        #endregion

        private void OnDestroy()
        {
            Debug.Log("OnDestroy");
            if (RtcEngine == null) return;
            RtcEngine.InitEventHandler(null);
            RtcEngine.LeaveChannel();
            RtcEngine.Dispose();
        }

        internal string GetChannelName()
        {
            return _channelName;
        }

        #region -- Video Render UI Logic ---

        internal void MakeVideoView(uint uid, string channelId = "")
        {
            var go = GameObject.Find(uid.ToString());
            if (!ReferenceEquals(go, null))
            {
                return; // reuse
            }

            VideoSurface videoSurface = MakeImageSurface(uid.ToString());

            if (ReferenceEquals(videoSurface, null)) return;
            // configure videoSurface
            if (uid == 0)
            {
                videoSurface.SetForUser(uid, channelId);
            }
            else
            {
                videoSurface.SetForUser(uid, channelId, VIDEO_SOURCE_TYPE.VIDEO_SOURCE_REMOTE);
            }


            videoSurface.OnTextureSizeModify += (int width, int height) =>
            {
                var transform = videoSurface.GetComponent<RectTransform>();
                if (transform)
                {
                    //If render in RawImage. just set rawImage size.
                    transform.sizeDelta = new Vector2(width / 2, height / 2);
                    transform.localScale = Vector3.one;
                }
                else
                {
                    //If render in MeshRenderer, just set localSize with MeshRenderer
                    float scale = (float)height / (float)width;
                    videoSurface.transform.localScale = new Vector3(-1, 1, scale);
                }
                Debug.Log("OnTextureSizeModify: " + width + "  " + height);
            };

            videoSurface.SetEnable(true);
        }

        // VIDEO TYPE 1: 3D Object
        private VideoSurface MakePlaneSurface(string goName)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Plane);

            if (go == null)
            {
                return null;
            }

            go.name = goName;
            // set up transform
            go.AddComponent<UIElementDrag>();

            var father = GameObject.Find("3DParent");
            if (father != null)
            {
                go.transform.SetParent(father.transform);
                go.transform.Rotate(-90.0f, 0.0f, 0.0f);
                var random = new System.Random();
                go.transform.position = new Vector3(random.Next(-100, 100), random.Next(-100, 100), 0);
                go.transform.localScale = new Vector3(10, 10, 10);
            }

            var meshRenderer = go.GetComponent<MeshRenderer>();
            var shader = Shader.Find("Unlit/Texture");
            if (shader != null)
            {
                meshRenderer.material = new Material(shader);
            }
            else
            {
                Log.UpdateLog("It looks like Unlit/Texture shader not include Always Includes Shaders. May be some videosurface will be pink");
            }

            // configure videoSurface
            VideoSurface videoSurface = go.AddComponent<VideoSurfaceYUVA>();

            return videoSurface;
        }

        // Video TYPE 2: RawImage
        private VideoSurface MakeImageSurface(string goName)
        {
            GameObject go = new GameObject();

            if (go == null)
            {
                return null;
            }

            go.name = goName;
            // to be renderered onto
            go.AddComponent<RawImage>();
            // make the object draggable
            go.AddComponent<UIElementDrag>();
            var canvas = GameObject.Find("VideoCanvas");
            if (canvas != null)
            {
                go.transform.SetParent(canvas.transform);
                Debug.Log("add video view");
            }
            else
            {
                Debug.Log("Canvas is null video view");
            }

            // set up transform
            go.transform.Rotate(0f, 0.0f, 180.0f);
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = new Vector3(2f, 3f, 1f);

            // configure videoSurface
            VideoSurface videoSurface = go.AddComponent<VideoSurfaceYUVA>();

            return videoSurface;
        }

        internal void DestroyVideoView(uint uid)
        {
            var go = GameObject.Find(uid.ToString());
            if (!ReferenceEquals(go, null))
            {
                Destroy(go);
            }
        }

        # endregion
    }

    #region -- Agora Event ---





    internal class UserEventHandler : IRtcEngineEventHandler
    {
        private readonly RenderWithAlphaSample _sample;

        internal UserEventHandler(RenderWithAlphaSample videoSample)
        {
            _sample = videoSample;
        }

        public override void OnError(int err, string msg)
        {
            _sample.Log.UpdateLog(string.Format("OnError err: {0}, msg: {1}", err, msg));
        }

        public override void OnJoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            int build = 0;
            Debug.Log("Agora: OnJoinChannelSuccess ");
            _sample.Log.UpdateLog(string.Format("sdk version: ${0}",
                _sample.RtcEngine.GetVersion(ref build)));
            _sample.Log.UpdateLog(string.Format("sdk build: ${0}",
              build));
            _sample.Log.UpdateLog(
                string.Format("OnJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}",
                                connection.channelId, connection.localUid, elapsed));

            _sample.MakeVideoView(0, "");
        }

        public override void OnRejoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            _sample.Log.UpdateLog("OnRejoinChannelSuccess");
        }

        public override void OnLeaveChannel(RtcConnection connection, RtcStats stats)
        {
            _sample.DestroyVideoView(0);
        }

        public override void OnClientRoleChanged(RtcConnection connection, CLIENT_ROLE_TYPE oldRole, CLIENT_ROLE_TYPE newRole, ClientRoleOptions newRoleOptions)
        {
            _sample.Log.UpdateLog("OnClientRoleChanged");
        }

        public override void OnUserJoined(RtcConnection connection, uint uid, int elapsed)
        {
            _sample.Log.UpdateLog(string.Format("OnUserJoined uid: ${0} elapsed: ${1}", uid, elapsed));
            _sample.MakeVideoView(uid, _sample.GetChannelName());
        }

        public override void OnUserOffline(RtcConnection connection, uint uid, USER_OFFLINE_REASON_TYPE reason)
        {
            _sample.Log.UpdateLog(string.Format("OnUserOffLine uid: ${0}, reason: ${1}", uid,
                (int)reason));
            _sample.DestroyVideoView(uid);
        }
    }

    # endregion
}