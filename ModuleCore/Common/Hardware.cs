using System;
using System.Collections.Generic;
using System.Management;
using System.Text;

namespace ModuleCore.Common
{
    public enum HardwareProfileComponents
    {
        ComputerModel,
        VolumeSerial,
        CpuId,
        MemoryCapacity,
        VideoControllerDescription
    }

    public class HardWare
    {
        public static Dictionary<string, string> HardwareProfile()
        {
            var retval = new Dictionary<string, string>
                         {
                             {HardwareProfileComponents.ComputerModel.ToString(), GetComputerModel()},
                             {HardwareProfileComponents.VolumeSerial.ToString(), GetVolumeSerial()},
                             {HardwareProfileComponents.CpuId.ToString(), GetCpuId()},
                             {HardwareProfileComponents.MemoryCapacity.ToString(), GetMemoryAmount()},
                             {HardwareProfileComponents.VideoControllerDescription.ToString(), GetVideoControllerDescription()}
                         };
            return retval;
        }

        private static string GetVideoControllerDescription()
        {
            var s1 = new ManagementObjectSearcher("select * from Win32_VideoController");
            foreach (ManagementObject oReturn in s1.Get())
            {
                var desc = oReturn["AdapterRam"];
                if (desc == null) continue;
                return oReturn["Description"].ToString().Trim();
            }
            return string.Empty;
        }

        private static string GetComputerModel()
        {
            var s1 = new ManagementObjectSearcher("select * from Win32_ComputerSystem");
            foreach (ManagementObject oReturn in s1.Get())
            {
                return oReturn["Model"].ToString().Trim();
            }
            return string.Empty;
        }

        private static string GetMemoryAmount()
        {
            var s1 = new ManagementObjectSearcher("select * from Win32_PhysicalMemory");
            foreach (ManagementObject oReturn in s1.Get())
            {
                return oReturn["Capacity"].ToString().Trim();
            }
            return string.Empty;
        }

        private static string GetVolumeSerial()
        {
            var disk = new ManagementObject(@"win32_logicaldisk.deviceid=""c:""");
            disk.Get();

            string volumeSerial = disk["VolumeSerialNumber"].ToString();
            disk.Dispose();

            return volumeSerial;
        }

        public static string GetCpuId()
        {
            var managClass = new ManagementClass("win32_processor");
            var managCollec = managClass.GetInstances();

            foreach (ManagementObject managObj in managCollec)
            {
                //Get only the first CPU's ID
                return managObj.Properties["processorID"].Value.ToString();
            }
            return string.Empty;
        }

        public static string GetRegist()
        {
            var cpuid = GetCpuId();

            var inputByteArray = new byte[cpuid.Length];
            for (var x = 0; x < inputByteArray.Length; x++)
            {
                var i = Convert.ToInt32(cpuid.Substring(x, 1), 16);
                inputByteArray[x] = (byte)(16 - i);
            }

            List<byte> registbytes = new List<byte>();
            for (int i = 0; i < inputByteArray.Length / 2; i++)
            {
                var b = (inputByteArray[i] + inputByteArray[inputByteArray.Length - i - 1]) / 2;
                b = b > 9 ? 9 : b;
                registbytes.Add((byte)b);
            }

            StringBuilder stringBuilder = new();

            foreach (var item in registbytes)
            {
                stringBuilder.Append(item.ToString("X"));
            }

            return stringBuilder.ToString();
        }
    }
}