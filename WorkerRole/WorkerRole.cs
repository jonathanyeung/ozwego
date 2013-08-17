using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace WorkerRole
{
    public class WorkerRole : RoleEntryPoint
    {
        private Thread _listenerThread;

        public static RoomManager RoomManager;
        public static ClientManager ClientManager;
        public static IncomingMessageHandler IncomingMessageHandler;
        public static MessageSender MessageSender;

        public override void Run()
        {
            // This is a sample worker implementation. Replace with your logic.
            Trace.WriteLine("WorkerRole entry point called", "Information");

            while (true)
            {
                Thread.Sleep(1000);
                Trace.WriteLine("Working", "Information");
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections 
            //ToDo: Increase this number? !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            //
            // Instantiate Singletons
            //

            RoomManager = RoomManager.GetRoomManager();
            ClientManager = ClientManager.GetClientManager();
            IncomingMessageHandler = IncomingMessageHandler.GetIncomingMessageHandler();
            MessageSender = MessageSender.GetMessageSender();


            StartListenerThread();

            return base.OnStart();
        }

        private void StartListenerThread()
        {
            _listenerThread = null;
            _listenerThread = new Thread(TCPListener.TcpListenerProcess);
            _listenerThread.TrySetApartmentState(ApartmentState.STA);
            _listenerThread.Name = "Server TCP Listener";
            _listenerThread.Start();
        }
    }
}
