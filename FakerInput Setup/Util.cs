using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Deployment.WindowsInstaller;

namespace FakerInput_Setup
{
    static class Util
    {
        public static Guid sysGuid = new Guid("{4D36E97D-E325-11CE-BFC1-08002BE10318}");
        public static Guid keyboardGuid = new Guid("{884b96c3-56ef-11d1-bc8c-00a0c91405dd}");
        public static Guid keyboardClassGuid = new Guid("{4d36e96b-e325-11ce-bfc1-08002be10318}");
        public static string exepath = Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName;
        public static string arch = Environment.Is64BitOperatingSystem ? "x64" : "x86";
        internal const string fakerInputDevicePath = @"root\FakerInput";

        public static bool IsFakerInputInstalled()
        {
            return CheckForSysDevice(fakerInputDevicePath);
        }

        private static bool CheckForSysDevice(string searchHardwareId)
        {
            bool result = false;
            Guid sysGuid = Guid.Parse("{4d36e97d-e325-11ce-bfc1-08002be10318}");
            NativeMethods.SP_DEVINFO_DATA deviceInfoData = new NativeMethods.SP_DEVINFO_DATA();
            deviceInfoData.cbSize = System.Runtime.InteropServices.Marshal.SizeOf(deviceInfoData);
            var dataBuffer = new byte[4096];
            ulong propertyType = 0;
            var requiredSize = 0;
            //var type = 0;
            IntPtr deviceInfoSet = NativeMethods.SetupDiGetClassDevs(ref sysGuid, null, 0, 0);
            for (int i = 0; !result && NativeMethods.SetupDiEnumDeviceInfo(deviceInfoSet, i, ref deviceInfoData); i++)
            {
                if (NativeMethods.SetupDiGetDeviceProperty(deviceInfoSet, ref deviceInfoData, ref NativeMethods.DEVPKEY_Device_HardwareIds, ref propertyType,
                    dataBuffer, dataBuffer.Length, ref requiredSize, 0))
                //if (NativeMethods.SetupDiGetDeviceRegistryProperty(deviceInfoSet, ref deviceInfoData, NativeMethods.SPDRP_DEVICEDESC, ref type,
                //    dataBuffer, dataBuffer.Length, ref requiredSize))
                {
                    string hardwareId = dataBuffer.ToUTF16String();
                    if (hardwareId.Equals(searchHardwareId))
                        result = true;

                    //Console.WriteLine(dataBuffer.ToUTF8String());
                    //Console.WriteLine(hardwareId);
                }
            }

            if (deviceInfoSet.ToInt64() != NativeMethods.INVALID_HANDLE_VALUE)
            {
                NativeMethods.SetupDiDestroyDeviceInfoList(deviceInfoSet);
            }

            return result;
        }

        public static string FakerInputInstanceId()
        {
            return ObtainSysDeviceInstanceId(fakerInputDevicePath);
        }

        public static string FakerInputDevProp(Session session, NativeMethods.DEVPROPKEY prop)
        {
            return GetDriverProperty(session, sysGuid, FakerInputInstanceId(), prop);
        }

        public static string GetDriverProperty(Session session, Guid searchGuid, string searchHardwareId,
            NativeMethods.DEVPROPKEY prop)
        {
            string result = "";
            bool devmatch = false;
            //Guid sysGuid = Guid.Parse("{4d36e97d-e325-11ce-bfc1-08002be10318}");
            NativeMethods.SP_DEVINFO_DATA deviceInfoData = new NativeMethods.SP_DEVINFO_DATA();
            deviceInfoData.cbSize = System.Runtime.InteropServices.Marshal.SizeOf(deviceInfoData);
            var dataBuffer = new byte[4096];
            ulong propertyType = 0;
            var requiredSize = 0;
            //var type = 0;
            IntPtr deviceInfoSet = NativeMethods.SetupDiGetClassDevs(ref searchGuid, null, 0, 0);
            for (int i = 0; !devmatch && NativeMethods.SetupDiEnumDeviceInfo(deviceInfoSet, i, ref deviceInfoData); i++)
            {
                //session.Log($"ITER {i}");
                if (NativeMethods.SetupDiGetDeviceProperty(deviceInfoSet, ref deviceInfoData, ref NativeMethods.DEVPKEY_Device_InstanceId, ref propertyType,
                    dataBuffer, dataBuffer.Length, ref requiredSize, 0))
                {
                    string currentInstanceId = dataBuffer.ToUTF16String();
                    session.Log($"Search device {currentInstanceId}");
                    if (currentInstanceId.Equals(searchHardwareId))
                        devmatch = true;
                }
            }

            if (devmatch)
            {
                //session.Log("FOUND MATCH");
                if (NativeMethods.SetupDiGetDeviceProperty(deviceInfoSet, ref deviceInfoData, ref prop, ref propertyType,
                    dataBuffer, dataBuffer.Length, ref requiredSize, 0))
                {
                    result = dataBuffer.ToUTF16String();
                }
            }

            if (deviceInfoSet.ToInt64() != NativeMethods.INVALID_HANDLE_VALUE)
            {
                NativeMethods.SetupDiDestroyDeviceInfoList(deviceInfoSet);
            }

            return result;
        }

        private static string ObtainSysDeviceInstanceId(string searchHardwareId)
        {
            string result = "";
            bool devmatch = false;
            Guid sysGuid = Guid.Parse("{4d36e97d-e325-11ce-bfc1-08002be10318}");
            NativeMethods.SP_DEVINFO_DATA deviceInfoData = new NativeMethods.SP_DEVINFO_DATA();
            deviceInfoData.cbSize = System.Runtime.InteropServices.Marshal.SizeOf(deviceInfoData);
            var dataBuffer = new byte[4096];
            ulong propertyType = 0;
            var requiredSize = 0;
            //var type = 0;
            IntPtr deviceInfoSet = NativeMethods.SetupDiGetClassDevs(ref sysGuid, null, 0, 0);
            for (int i = 0; !devmatch && NativeMethods.SetupDiEnumDeviceInfo(deviceInfoSet, i, ref deviceInfoData); i++)
            {
                if (NativeMethods.SetupDiGetDeviceProperty(deviceInfoSet, ref deviceInfoData, ref NativeMethods.DEVPKEY_Device_HardwareIds, ref propertyType,
                    dataBuffer, dataBuffer.Length, ref requiredSize, 0))
                {
                    string hardwareId = dataBuffer.ToUTF16String();
                    if (hardwareId.Equals(searchHardwareId))
                        devmatch = true;
                }
            }

            if (devmatch)
            {
                if (NativeMethods.SetupDiGetDeviceProperty(deviceInfoSet, ref deviceInfoData, ref NativeMethods.DEVPKEY_Device_InstanceId, ref propertyType,
                    dataBuffer, dataBuffer.Length, ref requiredSize, 0))
                {
                    result = dataBuffer.ToUTF16String();
                }
            }

            if (deviceInfoSet.ToInt64() != NativeMethods.INVALID_HANDLE_VALUE)
            {
                NativeMethods.SetupDiDestroyDeviceInfoList(deviceInfoSet);
            }

            return result;
        }
    }
}
