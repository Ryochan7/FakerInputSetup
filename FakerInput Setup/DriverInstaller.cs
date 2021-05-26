using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Deployment.WindowsInstaller;
using Nefarius.Devcon;

namespace FakerInput_Setup
{
    public class DriverInstaller
    {
        [CustomAction]
        public static ActionResult InstallAction(Session session)
        {
            ActionResult result = ActionResult.Failure;
            //return result;

            session.Log("Creating FakerInput system device");

            session.Log("Installing FakerInput driver");
            string infPath = Path.Combine(session.CustomActionData["INSTALLDIR"], "FakerInput.inf");
            session.Log(infPath);
            //System.Threading.Thread.Sleep(10000);

            //return result;

            bool deviceCreated = Devcon.Create("System", Util.sysGuid, Util.fakerInputDevicePath);
            
            bool driverInstalled = Devcon.Install(infPath, out bool rebootNeeded);
            //session.Log(driverInstalled.ToString());
            if (driverInstalled)
            {
                result = ActionResult.Success;
            }
            else
            {
                // Driver install cancelled or failed. Remove virtual device
                string instanceId = Util.FakerInputInstanceId();
                if (!string.IsNullOrEmpty(instanceId))
                {
                    Devcon.Remove(Util.sysGuid, instanceId);
                }
            }

            //System.Threading.Thread.Sleep(5000);

            return result;
        }

        [CustomAction]
        public static ActionResult RemoveAction(Session session)
        {
            ActionResult result = ActionResult.Failure;

            session.Log("Removing FakerInput system device");

            string infFile = Util.FakerInputDevProp(session, NativeMethods.DEVPKEY_Device_DriverInfPath);
            string instanceId = Util.FakerInputInstanceId();
            bool removeTemp = false;

            // If instance ID is not found, assume virtual devices do not exist
            if (!string.IsNullOrEmpty(instanceId))
            {
                //session.Log("TEST INSTANCE ID");
                //session.Log(instanceId);
                //session.Log("");

                // Check for presense of virtual keyboard controlled by FakerInput
                int deviceIdx = 0;
                string foundKeyboardInstanceId = string.Empty;
                bool foundKeyboard = false;
                while (Devcon.Find(Util.keyboardGuid, out string tempPath,
                    out string tempInstanceId, deviceIdx) && !foundKeyboard)
                {
                    //session.Log(deviceIdx.ToString());
                    //session.Log(tempInstanceId);
                    //System.Threading.Thread.Sleep(1000);
                    string tempStr = Util.GetDriverProperty(session, new Guid("{4d36e96b-e325-11ce-bfc1-08002be10318}"), tempInstanceId, NativeMethods.DEVPKEY_Device_Parent);
                    //session.Log("WHAT IS THIS");
                    //session.Log(tempStr);
                    //System.Threading.Thread.Sleep(1000);
                    if (tempStr == instanceId)
                    {
                        //session.Log("PINEAPPLES");
                        foundKeyboardInstanceId = tempInstanceId;
                        foundKeyboard = true;
                        //Devcon.Remove(Util.keyboardClassGuid, foundKeyboardInstanceId);
                        //System.Threading.Thread.Sleep(3000);
                    }

                    deviceIdx++;
                }

                // Have to remove virtual Keyboard device first as Windows has
                // an exclusive hold on it. The virtual system device removal would fail
                if (foundKeyboard)
                {
                    Devcon.Remove(Util.keyboardClassGuid, foundKeyboardInstanceId);
                }

                // Remove system FakerInput device
                removeTemp = Devcon.Remove(Util.sysGuid, instanceId);
            }

            bool infFileFound = !string.IsNullOrEmpty(infFile);
            if (removeTemp && infFileFound)
            {
                session.Log($"Deleting driver from Driver Store {infFile}");
                bool driverRemoved = NativeMethods.SetupUninstallOEMInfW(infFile, 0x0001, IntPtr.Zero);
                if (driverRemoved)
                {
                    result = ActionResult.Success;
                }
            }
            else if (!removeTemp && !infFileFound)
            {
                result = ActionResult.Success;
            }

            return result;
        }
    }
}
