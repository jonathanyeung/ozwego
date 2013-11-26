using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using Microsoft.WindowsAzure.ServiceRuntime;
using WorkerRole.Datacore;

namespace WorkerRole
{

    public class WorkerRole : RoleEntryPoint
    {
        private Thread _listenerThread;

        public override void Run()
        {
            Trace.WriteLine("++WorkerRole.Run", "Information");

            while (true)
            {
                Thread.Sleep(30000);
                Trace.WriteLine("Server Heartbeat", "Information");
            }

            Trace.WriteLine("--WorkerRole.Run", "Information");
        }

        public override bool OnStart()
        {
            Trace.WriteLine("++WorkerRole.OnStart", "Information");

            // Set the maximum number of concurrent connections 
            //ToDo: Increase this number? !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            StartListenerThread();

            return base.OnStart();
            Trace.WriteLine("--WorkerRole.OnStart", "Information");
        }

        private void StartListenerThread()
        {
            Trace.WriteLine("++WorkerRole.StartListenerThread", "Information");

            _listenerThread = null;
            _listenerThread = new Thread(TCPListener.TcpListenerProcess);
            _listenerThread.TrySetApartmentState(ApartmentState.STA);
            _listenerThread.Name = "Server TCP Listener";
            _listenerThread.Start();

            Trace.WriteLine("--WorkerRole.StartListenerThread", "Information");
        }
    }
}
