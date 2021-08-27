using MvCamCtrl.NET;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace OpencvsharpModule.Devices
{
    public class MVSCameras : ICameras
    {
        public MVSCameras()
        {
            AppDomain.CurrentDomain.ProcessExit += OnExit;
        }

        /// <summary>
        /// ch:枚举 GIGE 设备 | en:Enum GIGE device
        /// </summary>
        /// <param name="needNum">指定需要发现的相机的数量</param>
        public async void InitCameras(int needNum)
        {
            await Task.Delay(100);
            uint deviceNum;
            do
            {
                GC.Collect();
                _ = MyCamera.MV_CC_EnumDevices_NET(MyCamera.MV_GIGE_DEVICE, ref StDevList);
                deviceNum = StDevList.nDeviceNum;
                if (deviceNum < needNum)
                {
                    ErrorMessage?.Invoke("未找到足够数量的相机,继续查找中……");
                    await Task.Delay(1000);
                }
            }
            while (deviceNum < needNum);
            if (deviceNum < 1) { ErrorMessage?.Invoke("未找到相机！"); return; }
            Initdevices();
        }

        public MyCamera.MV_CC_DEVICE_INFO_LIST StDevList = new();

        public event Action<string> ErrorMessage;

        public event Action<string> CameraListChanged;

        public Dictionary<string, MyCamera> CameraList = new();
        public Dictionary<string, Rect> FullRectList = new();

        /// <summary>
        /// 初始化相机
        /// </summary>
        private void Initdevices()
        {
            int nRet;
            // ch:创建设备 | en:Create device
            for (uint i = 0; i < StDevList.nDeviceNum; i++)
            {
                var stDevInfo = (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(StDevList.pDeviceInfo[i], typeof(MyCamera.MV_CC_DEVICE_INFO));

                MyCamera device = new();

                nRet = device.MV_CC_CreateDevice_NET(ref stDevInfo);

                MyCamera.MV_CC_DEVICE_INFO deviceinfo = (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(StDevList.pDeviceInfo[i], typeof(MyCamera.MV_CC_DEVICE_INFO));
                MyCamera.MV_GIGE_DEVICE_INFO gigeInfo = (MyCamera.MV_GIGE_DEVICE_INFO)MyCamera.ByteToStruct(deviceinfo.SpecialInfo.stGigEInfo, typeof(MyCamera.MV_GIGE_DEVICE_INFO));
                if (gigeInfo.chManufacturerName != "Hikrobot" && gigeInfo.chManufacturerName != "Hikvision") continue;
                //绑定相机========================================================================================================
                //if (gigeInfo.chSerialNumber != "00F63387250" && gigeInfo.chSerialNumber != "00D57802787") { ErrorMessage?.Invoke("找不到相机,请联系厂家：17551023102"); return; }

                    // ch:显示IP | en:Display IP
                    if (nRet == MyCamera.MV_OK)
                {
                    uint nIp1 = (gigeInfo.nCurrentIp & 0xFF000000) >> 24;
                    uint nIp2 = (gigeInfo.nCurrentIp & 0x00FF0000) >> 16;
                    uint nIp3 = (gigeInfo.nCurrentIp & 0x0000FF00) >> 8;
                    uint nIp4 = (gigeInfo.nCurrentIp & 0x000000FF);
                    if (!CameraList.ContainsKey($"{nIp1}.{nIp2}.{nIp3}.{nIp4}"))
                    {
                        CameraList.Add($"{nIp1}.{nIp2}.{nIp3}.{nIp4}", device);
                        CameraListChanged?.Invoke($"Hikrobot;{nIp1}.{nIp2}.{nIp3}.{nIp4}");
                    }
                }
                else
                {
                    ShowErrorMsg("创建设备", nRet);
                }
            }

            LoadConfig();
        }

        //设置相机
        public void LoadConfig()
        {
            int nRet;

            foreach (var device in CameraList)
            {
                // ch:停止抓图 | en:Stop grab image
                _ = device.Value.MV_CC_StopGrabbing_NET();
                // ch:关闭设备 | en:Close device
                _ = device.Value.MV_CC_CloseDevice_NET();
                // ch:打开设备 | en:Open device
                nRet = device.Value.MV_CC_OpenDevice_NET();
                if (nRet != MyCamera.MV_OK) ShowErrorMsg("打开设备", nRet);

                // ch:触发源选择:0 - Line0; | en:Trigger source select:0 - Line0;
                //           1 - Line1;
                //           2 - Line2;
                //           3 - Line3;
                //           4 - Counter;
                //           7 - Software;
                _ = device.Value.MV_CC_SetEnumValue_NET("TriggerMode", (uint)MyCamera.MV_CAM_TRIGGER_MODE.MV_TRIGGER_MODE_ON);
                _ = device.Value.MV_CC_SetEnumValue_NET("TriggerSource", (uint)MyCamera.MV_CAM_TRIGGER_SOURCE.MV_TRIGGER_SOURCE_SOFTWARE);
                _ = device.Value.MV_CC_SetEnumValue_NET("PixelFormat", (uint)MyCamera.MvGvspPixelType.PixelType_Gvsp_BGR8_Packed);
                _ = device.Value.MV_CC_SetBalanceWhiteAuto_NET(0);

                // BalanceWhiteAuto MV_CC_SetBalanceWhiteAuto_NET
                //_ = device.Value.MV_CC_SetHeight_NET(2000);
                //_ = device.Value.MV_CC_SetAOIoffsetY_NET(0);

                // ch:探测网络最佳包大小(只对GigE相机有效) | en:Detection network optimal package size(It only works for the GigE camera)
                int nPacketSize = device.Value.MV_CC_GetOptimalPacketSize_NET();
                if (nPacketSize > 0)
                {
                    nRet = device.Value.MV_CC_SetIntValue_NET("GevSCPSPacketSize", (uint)nPacketSize);
                    if (nRet != MyCamera.MV_OK) ShowErrorMsg("探测网络最佳包大小", nRet);
                }

                //==================================================
                // ch:开启抓图 | en:start grab
                nRet = device.Value.MV_CC_StartGrabbing_NET();

                _ = device.Value.MV_CC_GetImageInfo_NET(ref pstInfo);
                _ = device.Value.MV_CC_GetAOIoffsetY_NET(ref pstValue);
                int offsetY = (int)pstValue.nCurValue;
                _ = device.Value.MV_CC_GetAOIoffsetX_NET(ref pstValue);
                int offsetX = (int)pstValue.nCurValue;
                var width = (int)pstInfo.nWidthMax + offsetX;
                var height = (int)pstInfo.nHeightMax + offsetY;

                FullRectList.Add(device.Key, new Rect(0, 0, width, height));

                if (nRet != MyCamera.MV_OK) ShowErrorMsg("开启抓图", nRet);

                _ = GetOneImage(device.Key, 1000, new(0, 0, 0, 0));
            }
        }

        private OpenCvSharp.Rect ROIRect;
        private OpenCvSharp.Rect ROIRectFomat;

        private void FomatROI(MyCamera camera)
        {
            var basicinfo = new MyCamera.MV_IMAGE_BASIC_INFO();
            _ = camera.MV_CC_GetImageInfo_NET(ref basicinfo);

            if (basicinfo.nWidthInc > 0)
            {
                ROIRectFomat.X = ROIRect.X - ROIRect.X % (int)basicinfo.nWidthInc;
                ROIRectFomat.Y = ROIRect.Y - ROIRect.Y % (int)basicinfo.nHeightInc;
                ROIRectFomat.Width = ROIRect.Width - ROIRect.Width % (int)basicinfo.nWidthInc;
                ROIRectFomat.Height = ROIRect.Height - ROIRect.Height % (int)basicinfo.nWidthInc;
                ROIRectFomat.Width = ROIRectFomat.Width < 32 ? 32 : ROIRectFomat.Width;
                ROIRectFomat.Height = ROIRectFomat.Height < 32 ? 32 : ROIRectFomat.Height;
            }
        }

        private MyCamera.MV_IMAGE_BASIC_INFO pstInfo;
        private MyCamera.MVCC_INTVALUE pstValue;
        private readonly object locko = new();


        //为了加快拍照速度，这里做了一个取AOI的功能，如果每次都是取固定的AOI,建议使用此功能，如果每次的AOI不同，还是全部取回再截取，因为切换AOI需要时间。
        public async Task<Mat> GetOneImage(string index, int exposureTime, Rect rect)
        {
            Mat mat = null;

            if (rect.Width == 0)
            {
                rect = FullRectList[index];
            }

            await Task.Run(() =>
        {
            lock (locko)
            {
                if (ROIRect != rect)
                {
                    ROIRect = rect;
                    FomatROI(CameraList[index]);
                    var r = CameraList[index].MV_CC_StopGrabbing_NET();
                    r = CameraList[index].MV_CC_SetWidth_NET(32);
                    r = CameraList[index].MV_CC_SetHeight_NET(32);
                    r = CameraList[index].MV_CC_SetAOIoffsetX_NET((uint)ROIRectFomat.Left);
                    r = CameraList[index].MV_CC_SetAOIoffsetY_NET((uint)ROIRectFomat.Top);
                    r = CameraList[index].MV_CC_SetWidth_NET((uint)ROIRectFomat.Width);
                    r = CameraList[index].MV_CC_SetHeight_NET((uint)ROIRectFomat.Height);
                    r = CameraList[index].MV_CC_StartGrabbing_NET();
                }
                //ExposureTime
                var nRet = CameraList[index].MV_CC_SetFloatValue_NET("ExposureTime", exposureTime);

                //Trigger
                nRet = CameraList[index].MV_CC_SetCommandValue_NET("TriggerSoftware");

                if (nRet != MyCamera.MV_OK) ShowErrorMsg("TriggerSoftware", nRet);

                MyCamera.MV_FRAME_OUT FrameInfo = new();
                do
                {
                    _ = CameraList[index].MV_CC_GetImageBuffer_NET(ref FrameInfo, 1);
                }
                while (FrameInfo.pBufAddr == IntPtr.Zero);

                // 转Mat

                var h = FrameInfo.stFrameInfo.nHeight;
                var w = FrameInfo.stFrameInfo.nWidth;
                if (IsMonoData(FrameInfo.stFrameInfo.enPixelType))
                {
                    //if (rect == FullRectList[index])
                    //{
                    //    mat = new Mat(h, w, MatType.CV_8U, FrameInfo.pBufAddr);
                    //    // mat = mat.CvtColor(ColorConversionCodes.GRAY2BGR);
                    //}
                    //else
                    //{
                    //    mat = new Mat(FullRectList[index].Height, FullRectList[index].Width, MatType.CV_8U);
                    //    mat[rect.Top, rect.Top + h, rect.Left, rect.Left + w] = new Mat(h, w, MatType.CV_8U, FrameInfo.pBufAddr);
                    //}

                    mat = new Mat(h, w, MatType.CV_8U, FrameInfo.pBufAddr);
                }
                else if (FrameInfo.stFrameInfo.enPixelType == MyCamera.MvGvspPixelType.PixelType_Gvsp_BGR8_Packed)
                {
                    //if (rect == FullRectList[index])
                    //{
                    //    mat = new Mat(h, w, MatType.CV_8UC3, FrameInfo.pBufAddr);
                    //}
                    //else
                    //{
                    //    mat = new Mat(FullRectList[index].Height, FullRectList[index].Width, MatType.CV_8UC3);
                    //    mat[rect.Top, rect.Top + h, rect.Left, rect.Left + w] = new Mat(h, w, MatType.CV_8UC3, FrameInfo.pBufAddr);
                    //}

                    mat = new Mat(h, w, MatType.CV_8UC3, FrameInfo.pBufAddr);
                }
                else
                    ErrorMessage?.Invoke("请将相机图片格式设置为 BGR8 ");

                if (FrameInfo.pBufAddr != IntPtr.Zero)
                {
                    _ = CameraList[index].MV_CC_FreeImageBuffer_NET(ref FrameInfo);
                }
            }
        });

            return mat;
        }

        private void OnExit(object sender, EventArgs e)
        {
            foreach (var device in CameraList)
            {
                // ch:停止抓图 | en:Stop grab image
                _ = device.Value.MV_CC_StopGrabbing_NET();
                // ch:关闭设备 | en:Close device
                _ = device.Value.MV_CC_CloseDevice_NET();
                // ch:销毁设备 | en:Destroy device
                _ = device.Value.MV_CC_DestroyDevice_NET();
            }
        }


        /************************************************************************
         *  @fn     IsColorData()
         *  @brief  判断是否是彩色数据
         *  @param  enGvspPixelType         [IN]           像素格式
         *  @return 成功，返回0；错误，返回-1
         ************************************************************************/

        public static bool IsColorData(MyCamera.MvGvspPixelType enGvspPixelType)
        {
            return enGvspPixelType switch
            {
                MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGR8
                or MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerRG8
                or MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGB8
                or MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerBG8
                or MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGR10
                or MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerRG10
                or MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGB10
                or MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerBG10
                or MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGR12
                or MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerRG12
                or MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGB12
                or MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerBG12
                or MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGR10_Packed
                or MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerRG10_Packed
                or MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGB10_Packed
                or MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerBG10_Packed
                or MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGR12_Packed
                or MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerRG12_Packed
                or MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGB12_Packed
                or MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerBG12_Packed
                or MyCamera.MvGvspPixelType.PixelType_Gvsp_RGB8_Packed
                or MyCamera.MvGvspPixelType.PixelType_Gvsp_YUV422_Packed
                or MyCamera.MvGvspPixelType.PixelType_Gvsp_YUV422_YUYV_Packed
                or MyCamera.MvGvspPixelType.PixelType_Gvsp_YCBCR411_8_CBYYCRYY
                => true,
                _ => false,
            };
        }

        /************************************************************************
         *  @fn     IsMonoData()
         *  @brief  判断是否是彩色数据
         *  @param  enGvspPixelType         [IN]           像素格式
         *  @return 成功，返回0；错误，返回-1
         ************************************************************************/

        public static bool IsMonoData(MyCamera.MvGvspPixelType enGvspPixelType)
        {
            return enGvspPixelType switch
            {
                MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono8 or MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono10 or MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono10_Packed or MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono12 or MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono12_Packed => true,
                _ => false,
            };
        }

        private string ErrorMsg
        {
            get; set;
        }

        // ch:显示错误信息 | en:Show error message
        private void ShowErrorMsg(string csMessage, int nErrorNum)
        {
            if (nErrorNum == 0)
            {
                ErrorMsg += csMessage;
            }
            else
            {
                ErrorMsg += csMessage + ": Error =" + String.Format("{0:X}", nErrorNum);
            }

            switch (nErrorNum)
            {
                case MyCamera.MV_E_HANDLE: ErrorMsg += " Error or invalid handle "; break;
                case MyCamera.MV_E_SUPPORT: ErrorMsg += " Not supported function "; break;
                case MyCamera.MV_E_BUFOVER: ErrorMsg += " Cache is full "; break;
                case MyCamera.MV_E_CALLORDER: ErrorMsg += " Function calling order error "; break;
                case MyCamera.MV_E_PARAMETER: ErrorMsg += " Incorrect parameter "; break;
                case MyCamera.MV_E_RESOURCE: ErrorMsg += " Applying resource failed "; break;
                case MyCamera.MV_E_NODATA: ErrorMsg += " No data "; break;
                case MyCamera.MV_E_PRECONDITION: ErrorMsg += " Precondition error, or running environment changed "; break;
                case MyCamera.MV_E_VERSION: ErrorMsg += " Version mismatches "; break;
                case MyCamera.MV_E_NOENOUGH_BUF: ErrorMsg += " Insufficient memory "; break;
                case MyCamera.MV_E_UNKNOW: ErrorMsg += " Unknown error "; break;
                case MyCamera.MV_E_GC_GENERIC: ErrorMsg += " General error "; break;
                case MyCamera.MV_E_GC_ACCESS: ErrorMsg += " Node accessing condition error "; break;
                case MyCamera.MV_E_ACCESS_DENIED: ErrorMsg += " No permission "; break;
                case MyCamera.MV_E_BUSY: ErrorMsg += " Device is busy, or network disconnected "; break;
                case MyCamera.MV_E_NETER: ErrorMsg += " Network error "; break;
            }
            ErrorMsg += "\n";
            ErrorMessage?.Invoke(ErrorMsg);
        }
    }
}