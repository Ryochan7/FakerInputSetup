using System;
using System.Collections.Generic;
using WixSharp;

namespace FakerInput_Setup
{
    class Program
    {
        static void Main()
        {
            Project project = new Project("FakerInput",
                                new LaunchCondition("VersionNT >= 603",
                                    "This driver only works on Windows 8.1 and later. This is due to the need for at least UMDF 2.0 support."),
                                new Dir(@"%ProgramFiles%\Ryochan7\FakerInput",
                                    new File(@"Files\FakerInput.dll"),
                                    new File(@"Files\fakerinput.cat"),
                                    new File(new Id("DRIVER_INF_FILE"), @"Files\FakerInput.inf"))
                                );

            project.GUID = new Guid("17AA3E01-1012-4BF7-B908-1C4999F99259");
            project.UpgradeCode = new Guid("BF63C434-BF91-4666-B817-AD7B5C249E91");
            //project.SourceBaseDir = "<input dir path>";
            //project.OutDir = "<output dir path>";
#if WIN64
            project.Platform = Platform.x64;
#endif

            project.DefaultRefAssemblies = new List<string>
            {
                "ManagedDevcon.dll"
            };

            project.Actions = new WixSharp.Action[]
            {
                new ElevatedManagedAction(DriverInstaller.InstallAction, Return.check,
                    When.Before, Step.InstallFinalize, Condition.NOT_Installed)
                {
                    Execute = Execute.deferred,
                    UsesProperties = "INSTALLDIR",
                },

                new ElevatedManagedAction(DriverInstaller.RemoveAction, Return.check,
                    When.After, Step.InstallInitialize, Condition.BeingUninstalled)
                {
                    Execute = Execute.deferred,
                }
            };

            //project.LaunchConditions = new List<LaunchCondition>()
            //{
            //    new LaunchCondition()
            //    {
            //        Message = "This driver only works on Windows 8.1 and later. This is due to the need for at least UMDF 2.0 support.",
            //        ComponentCondition = "VersionNT >= 603",
            //    }
            //};

            project.Version = new Version("0.1.1");
            project.ControlPanelInfo.Contact = "Ryochan7";
            project.ControlPanelInfo.Manufacturer = "Ryochan7";
            project.LicenceFile = @"Files\LICENSE.rtf";
            project.PreserveTempFiles = true;
            //project.InstallPrivileges = InstallPrivileges.elevated;
            project.InstallScope = InstallScope.perMachine;

            project.MajorUpgrade = new MajorUpgrade
            {
                AllowSameVersionUpgrades = true,
                AllowDowngrades = false,
                DowngradeErrorMessage = "A newer version of FakerInput was already found. Please uninstall the currently installed product first.",
                Schedule = UpgradeSchedule.afterInstallInitialize,
            };

            project.BuildMsi();
        }
    }
}