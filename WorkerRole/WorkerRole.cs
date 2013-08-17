using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Net.Sockets;
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

        // Unique Identifier for this worker role instance
        public static Guid instanceID;

        public static List<InternalClient> InternalClients = new List<InternalClient>();

        public override void Run()
        {
            // This is a sample worker implementation. Replace with your logic.
            Trace.WriteLine("WorkerRole entry point called", "Information");

            while (true)
            {
                //Thread.Sleep(1000);
                //Trace.WriteLine("Working", "Information");
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

            instanceID = Guid.NewGuid();


            //
            // Start a listener thread on the client-facing external endpoint
            //

            var ExternalBag = new TcpListenerPropertyBag
                {
                    endpoint = RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["Endpoint1"].IPEndpoint,
                    endpointType = EndpointType.External
                };

            StartListenerThread(ExternalBag);


            //
            // Start a listener thread for incoming connections from other internal endpoints
            //

            //var InternalBag = new TcpListenerPropertyBag
            //    {
            //        endpoint = RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["InternalEndpoint"].IPEndpoint,
            //        endpointType = EndpointType.Internal
            //    };

            //StartListenerThread(InternalBag);


            //
            // Enumerate through all other role instances, and try to establish a TCP
            // connection with each of them.
            //

            //var instances = RoleEnvironment.CurrentRoleInstance.InstanceEndpoints;
            //var curRole = RoleEnvironment.CurrentRoleInstance;

            //for (int i = 1; i < curRole.Role.Instances.Count; i++)
            //{
            //    Connect(curRole.Role.Instances[i].InstanceEndpoints["InternalEndpoint"].IPEndpoint);
            //}

            return base.OnStart();
        }


        private void StartListenerThread(TcpListenerPropertyBag bag)
        {
            _listenerThread = null;
            _listenerThread = new Thread(TCPListener.TcpListenerProcess);
            _listenerThread.TrySetApartmentState(ApartmentState.STA);
            _listenerThread.Name = string.Format("Server TCP Listener Type {0}",
                                                 Enum.GetName(typeof(EndpointType), bag.endpointType));
            _listenerThread.Start(bag);
        }


        private void Connect(IPEndPoint endpoint)
        {
            try
            {
                var connection = new TcpClient();
                connection.Connect(endpoint);

                var newClient = new InternalClient(connection.Client);
                InternalClients.Add(newClient);
            }
            catch (Exception)
            {
                Trace.WriteLine("Exception during WorkerRole.Connect");
            }
        }
    }
}
