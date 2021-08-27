using Basler.Pylon;
using OpenCvSharp;
using System;
using System.Collections.Generic;
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

        async public void InitCameras(int needNum = 0)
        {  // 获取所有相机信息
            int deviceNum;
            do
            {
                GC.Collect();
                allCameraInfo = CameraFinder.Enumerate();

                deviceNum = allCameraInfo.Count;
                if (deviceNum < needNum)
                {
                    ErrorMessage?.Invoke("未找到足够数量的相机,继续查找中……");
                    await Task.Delay(1000);
                }
            }
            while (deviceNum < needNum);

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
                CameraListChanged?.Invoke(type + ";" +ip);
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

        public async Task<Mat> GetOneImage(string ip, int exposureTime, Rect rect)
        {
         

            exposureTime = exposureTime < 35 ? 35 : exposureTime;
            exposureTime = exposureTime > 999985 ? 999985 : exposureTime;

            Mat mat = null;

            //Camera mCamera = allCameras.Where(c => c.CameraInfo[CameraInfoKey.DeviceIpAddress] == ip).FirstOrDefault();
            Camera mCamera = CameraList[ip];
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

            //设置要在采集之前
            mCamera.Parameters[PLCamera.ExposureAuto].SetValue("Off");  //自动曝光关
            mCamera.Parameters[PLCamera.ExposureTimeAbs].SetValue(exposureTime);
            //AOI set
           var maxwidth= mCamera.Parameters[PLCamera.WidthMax].GetValue();
           var maxHeight= mCamera.Parameters[PLCamera.HeightMax].GetValue();

            System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
            //Grab
            if (!mCamera.StreamGrabber.IsGrabbing)
                mCamera.StreamGrabber.Start(1);
            else
                goto fail;

            IGrabResult grabResult  = mCamera.StreamGrabber.RetrieveResult(5000, TimeoutHandling.Return);
 

            if (grabResult is null)
            {
                goto fail;
            }

            if (grabResult.GrabSucceeded)

            {
                // 转Mat
                var h = grabResult.Height;
                var w = grabResult.Width;
                mat = new Mat(h, w, MatType.CV_8UC1, grabResult.PixelDataPointer);
            }
            else

            {
                ErrorMessage?.Invoke($"Error: {grabResult.ErrorCode} {grabResult.ErrorDescription}");
                goto fail;
            }
            sw.Stop();
            System.Diagnostics.Debug.WriteLine( sw.ElapsedMilliseconds+"ms");
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