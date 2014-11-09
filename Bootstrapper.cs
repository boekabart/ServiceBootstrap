using System;
using System.Linq;
using System.ServiceProcess;
using System.Threading;

namespace ServiceBootstrap
{
    public static class Bootstrapper
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger
            (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static bool IsRunningOnMono()
        {
            return Type.GetType("Mono.Runtime") != null;
        }
#pragma warning disable 28
        public static void Main(string[] args, Func<string[], Action> doStartGetStop, string serviceName)
#pragma warning restore 28
        {
            if (IsRunningOnMono() || Environment.UserInteractive)
            {
                if (args.Any())
                {
                    MainWithArgs(args, serviceName);
                }
                else
                {
                    StartAsConsoleApp(doStartGetStop, args);
                }
            }
            else
            {
                StartAsWindowsService(doStartGetStop, serviceName);
            }
        }

        private static void StartAsConsoleApp(Func<string[], Action> doStartGetStop, string[] args)
        {
            Log.Debug("Console Mode");
            Log.Info("Starting");
            var doStop = doStartGetStop(args);
            Console.WriteLine("Press any key to quit");
            while (!Console.KeyAvailable)
                Thread.Sleep(100);
            while (Console.KeyAvailable)
                Console.ReadKey();
            Log.Info("Stopping");
            doStop();
        }

        private static void MainWithArgs(string[] args, string serviceName)
        {
            switch (args.First())
            {
                case "-install":
                    Console.WriteLine("Installing Service");
                    InstallRunnableService(serviceName);
                    break;
                case "-uninstall":
                    Console.WriteLine("Uninstalling Service");
                    UninstallRunnableService(serviceName);
                    break;
                case "-start":
                    Console.WriteLine("Starting Service");
                    StartExternalService(serviceName);
                    break;
                case "-stop":
                    Console.WriteLine("Stopping Service");
                    StopRunnableService(serviceName);
                    break;
                default:
                    Console.WriteLine("Possible parameters are: -start / -stop / -install / -uninstall");
                    break;
            }
        }

        private static void StartAsWindowsService(Func<string[],Action> doStartGetStop, string runnableServiceName )
        {
            Log.Debug("Service Mode");
            var servicesToRun = new ServiceBase[] { new WindowsService(doStartGetStop, runnableServiceName) };
            Log.Debug("Running Service");
            ServiceBase.Run(servicesToRun);
            Log.Debug("Ran Service");
        }

        private static void StartExternalService(string runnableServiceName)
        {
            if (TryAction(() => ServiceHelper.StartService(runnableServiceName), "Failed to start service: {0}"))
            {
                Log.Debug("Started service");
                Console.WriteLine("Started service");
            }
        }

        public static void StopRunnableService(string runnableServiceName)
        {
            if (TryAction(() => ServiceHelper.StopService(runnableServiceName), "Failed to stop service: {0}"))
            {
                Log.Debug("Stopped service");
                Console.WriteLine("Stopped service");
            }
        }

        public static void InstallRunnableService(string runnableServiceName)
        {
            if (TryAction(() => ServiceHelper.InstallService(runnableServiceName, runnableServiceName, null), "Failed to install as service: {0}"))
            {
                Log.Debug("Installed service");
                Console.WriteLine("Installed service");
            }
        }

        public static void UninstallRunnableService(string runnableServiceName)
        {
            if (TryAction(() => ServiceHelper.UninstallService(runnableServiceName), "Failed to uninstall as service: {0}"))
            {
                Log.Debug("Uninstalled service");
                Console.WriteLine("Uninstalled service");
            }
        }

        private static bool TryAction(Action action, string errorMessageTemplate)
        {
            try
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                action();
                Console.ForegroundColor = ConsoleColor.Gray;
                return true;
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine(errorMessageTemplate, e.Message);
                Console.ForegroundColor = ConsoleColor.Gray;
                return false;
            }
        }
    }
}
