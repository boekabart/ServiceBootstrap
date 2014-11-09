using System;
using System.Collections;
using System.Configuration.Install;
using System.ServiceProcess;

namespace RunnableServiceBootstrap
{
    internal class ServiceHelper
    {
        public static void ValidateServiceInstallation(string serviceName)
        {
            using (var controller = new ServiceController( serviceName))
            {
                try
                {
                    var status = controller.Status;
                }
                catch (Exception)
                {
                    throw new Exception("Service not installed");
                }
            }
        }

        public static void ValidateServiceNonInstallation(string serviceName)
        {
            using (var controller = new ServiceController(serviceName))
            {
                try
                {
                    var status = controller.Status;

                }
                catch (Exception)
                {
                    return;
                }
                throw new Exception("Service already installed");
            }
        }

        public static bool IsRunning(string serviceName)
        {
            using (var controller = new ServiceController(serviceName))
            {
                ValidateServiceInstallation(serviceName);
                return (controller.Status == ServiceControllerStatus.Running);
            }
        }

        private static string ExePath
        {
            get { return new Uri(System.Reflection.Assembly.GetEntryAssembly().GetName().CodeBase).LocalPath; }
        }

        public static void InstallService(string serviceName, string displayName, string description)
        {
            ValidateServiceNonInstallation(serviceName);

            var si = new ServiceInstaller();
            var spi = new ServiceProcessInstaller();
            si.Parent = spi;
            si.DisplayName = displayName;
            si.Description = description;
            si.ServiceName = serviceName;
            si.StartType = ServiceStartMode.Automatic;

            // update this if you want a different log
            si.Context = new InstallContext("install.log", null);
            si.Context.Parameters["assemblypath"] = ExePath;

            IDictionary stateSaver = new Hashtable();
            si.Install(stateSaver);
        }

        public static void UninstallService(string serviceName)
        {
            ValidateServiceInstallation(serviceName);

            var si = new ServiceInstaller();
            var spi = new ServiceProcessInstaller();
            si.Parent = spi;
            si.ServiceName = serviceName;

            // update this if you want a different log
            si.Context = new InstallContext("uninstall.log", null);
            si.Uninstall(null);
        }

        public static void StartService(string serviceName)
        {
            ValidateServiceInstallation(serviceName);

            using (var controller = new ServiceController(serviceName))
            {
                if (controller.Status != ServiceControllerStatus.Running)
                {
                    controller.Start();
                    controller.WaitForStatus(ServiceControllerStatus.Running,
                                                TimeSpan.FromSeconds(10));
                }
                else
                {
                    throw new Exception("Service already running");
                }
            }
        }

        public static void StopService(string serviceName)
        {
            ValidateServiceInstallation(serviceName);

            using (var controller = new ServiceController(serviceName))
            {
                if (controller.Status != ServiceControllerStatus.Stopped)
                {
                    controller.Stop();
                    controller.WaitForStatus(ServiceControllerStatus.Stopped,
                                                TimeSpan.FromSeconds(10));
                }
                else
                    throw new Exception("Service not running");
            }
        }
    }
}