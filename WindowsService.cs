using System;
using System.ServiceProcess;

namespace ServiceBootstrap
{
    public class WindowsService : ServiceBase
    {
        private readonly Func<string[],Action> _doStartGetStop;
        private Action _doStop;

        public WindowsService(Func<string[],Action> doStartGetStop, string serviceName)
        {
            _doStartGetStop = doStartGetStop;
                ServiceName =serviceName;
        }

        protected override void OnStart(string[] args)
        {
            _doStop = _doStartGetStop(args);
        }

        protected override void OnStop()
        {
            if (_doStop != null)
                _doStop();
        }
    }
}