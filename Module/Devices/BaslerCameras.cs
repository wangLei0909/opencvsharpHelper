using Basler.Pylon;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace OpencvsharpModule.Devices
{
    internal class BaslerCameras : ICameras
    {
        public event Action<string> ErrorMessage;

        public event Action<string> CameraListChanged;

        public BaslerCameras()
        {
            AppDomain.CurrentDomain.ProcessExit += OnExit;
        }


        [HandleProcessCorruptedStateExceptions]
        async public void InitCameras(int needNum = 0)
        {  // 获取所有相机信息
            int deviceNum = 0;  
            GC.Collect();
            do
            {
              try
                {
                    allCameraInfo = CameraFinder.Enumerate(); deviceNum = allCameraInfo.Count;
                    if (deviceNum < needNum)
                    {
                        ErrorMessage?.Invoke("未找到足够数量的相机,继续查找中……");
                        await Task.Delay(1000);
                    }
                }
                catch
                {
                    ErrorMessage?.Invoke("Basler运行时未安装");
                    return;
                }
            }
            while (deviceNum < needNum);
            if (deviceNum < 1) { ErrorMessage?.Invoke("未找到Basler相机！"); return; }

            Initdevices();
        }

        private void Initdevices()
        {
            for (int i = 0; i < allCameraInfo.Count; i++)
            {
                //实例化相机对象
                var mCamera = new Camera(allCameraInfo[i]);

                //打开相机连接
                mCamera.CameraOpened += Configuration.AcquireContinuous;

                allCameras.Add(mCamera);

                CameraList.Add(mCamera.CameraInfo[CameraInfoKey.DeviceIpAddress], mCamera);
                var type = mCamera.CameraInfo[CameraInfoKey.VendorName];
                var ip = mCamera.CameraInfo[CameraInfoKey.DeviceIpAddress];
                CameraListChanged?.Invoke(type + ";" + ip);
                LoadConfig();
            }
        }

        public void LoadConfig()
        {
            foreach (var mCamera in CameraList)
            {
                try
                {
                    mCamera.Value.Open();
                    mCamera.Value.Parameters[PLCamera.OffsetX].SetValue(0);
                    mCamera.Value.Parameters[PLCamera.OffsetY].SetValue(0);
                    var maxwidth = mCamera.Value.Parameters[PLCamera.WidthMax].GetValue();
                    var maxHeight = mCamera.Value.Parameters[PLCamera.HeightMax].GetValue();

                    FullRectList.Add(mCamera.Key, new Rect(0, 0, (int)maxwidth, (int)maxHeight));

                    mCamera.Value.Parameters[PLCameraInstance.MaxNumBuffer].SetValue(1);
                    mCamera.Value.Parameters[PLTransportLayer.HeartbeatTimeout]
                        .TrySetValue(1000, IntegerValueCorrection.Nearest);  // 1000 ms timeout
                }
                catch (Exception ex)
                {
                    ErrorMessage?.Invoke($"Error: {ex.Message}");
                }
            }
        }

        private OpenCvSharp.Rect ROIRect;
        private OpenCvSharp.Rect ROIRectFomat;
        public Dictionary<string, Rect> FullRectList = new();

        private void FomatROI(Camera camera)
        {
            ROIRectFomat = ROIRect;
            ROIRectFomat.Width = ROIRectFomat.Width < 64 ? 64 : ROIRectFomat.Width;
            ROIRectFomat.Height = ROIRectFomat.Height < 64 ? 64 : ROIRectFomat.Height;
        }

        public async Task<Mat> GetOneImage(string ip, int exposureTime, Rect rect)
        {
            exposureTime = exposureTime < 35 ? 35 : exposureTime;
            exposureTime = exposureTime > 999985 ? 999985 : exposureTime;

            Mat mat = null;

            Camera mCamera = CameraList[ip];

            if (rect.Width == 0)
            {         //AOI set
                rect = FullRectList[ip];
            }
            if (!mCamera.IsOpen)
            {
                try
                {
                    mCamera.Open();

                    mCamera.Parameters[PLCameraInstance.MaxNumBuffer].SetValue(1);
                    mCamera.Parameters[PLTransportLayer.HeartbeatTimeout]
                        .TrySetValue(1000, IntegerValueCorrection.Nearest);  // 1000 ms timeout
                }
                catch (Exception ex)
                {
                    ErrorMessage?.Invoke($"Error: {ex.Message}");
                    goto fail;
                }
            }
            if (ROIRect != rect)
            {
                ROIRect = rect;
                FomatROI(CameraList[ip]);
                CameraList[ip].Parameters[PLCamera.Width].SetValue(64);
                CameraList[ip].Parameters[PLCamera.Height].SetValue(64);
                CameraList[ip].Parameters[PLCamera.OffsetX].SetValue(ROIRectFomat.Left);
                CameraList[ip].Parameters[PLCamera.OffsetY].SetValue(ROIRectFomat.Top);
                CameraList[ip].Parameters[PLCamera.Width].SetValue(ROIRectFomat.Width);
                CameraList[ip].Parameters[PLCamera.Height].SetValue(ROIRectFomat.Height);
            }
            //设置要在采集之前
            mCamera.Parameters[PLCamera.ExposureAuto].SetValue("Off");  //自动曝光关
            mCamera.Parameters[PLCamera.ExposureTimeAbs].SetValue(exposureTime);

            //Grab
            if (!mCamera.StreamGrabber.IsGrabbing)
                mCamera.StreamGrabber.Start(1);
            else
                goto fail;
            IGrabResult grabResult = mCamera.StreamGrabber.RetrieveResult(5000, TimeoutHandling.Return);

            if (grabResult is null)
            {
                goto fail;
            }

            if (grabResult.GrabSucceeded)

            {
                // 转Mat
                if (mCamera.Parameters[PLCamera.PixelFormat].GetValue() == "Mono8")
                {
                    var h = grabResult.Height;
                    var w = grabResult.Width;
                    mat = new Mat(h, w, MatType.CV_8UC1, grabResult.PixelDataPointer);
                }
            }
            else

            {
                ErrorMessage?.Invoke($"Error: {grabResult.ErrorCode} {grabResult.ErrorDescription}");
                goto fail;
            }

            return mat;

        fail:
            await Task.Delay(100);
            return mat;
        }

        private List<ICameraInfo> allCameraInfo;//存储所有相机
        private readonly List<Camera> allCameras = new();
        public Dictionary<string, Camera> CameraList = new();

        private void OnExit(object sender, EventArgs e)
        {
            foreach (var camera in allCameras)
            {
                camera.StreamGrabber.Stop();
                camera.Close();
            }
        }
    }
}