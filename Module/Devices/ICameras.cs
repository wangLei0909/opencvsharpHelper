using OpenCvSharp;
using System;
using System.Threading.Tasks;

namespace OpencvsharpModule.Devices
{
    public interface ICameras
    {
        /// <summary>
        /// 异常信息
        /// </summary>
        public event Action<string> ErrorMessage;
        public void InitCameras(int needNum = 0);



        /// <summary>
        /// ch:获取一帧图像 | en:Get one image
        /// </summary>
        /// <param name="ip">指定相机</param>
        /// <param name="exposureTime">曝光时间ms</param>
        /// <returns></returns>
        public Task<Mat> GetOneImage(string ip, int exposureTime, Rect rect);

        public event Action<string> CameraListChanged;

    }
}
