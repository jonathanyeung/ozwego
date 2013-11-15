using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace WorkerRole
{
    public class TCPListener
    {
        /// <summary>
        /// TCP thread that listens for incoming connections.
        /// </summary>
        public static async void TcpListenerProcess()
        {
            Trace.WriteLine("TCP: Starting TcpListener...");

            //IPAddress addr = IPAddress.Loopback;

            try
            {
                //var listener = new TcpListener(
                //    RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["Endpoint1"].IPEndpoint);

                //listener.ExclusiveAddressUse = false;


                const int port = 4029;
                var addr = new IPAddress(new byte[] { 192, 168, 1, 2 });
                var listener = new TcpListener(addr, port);
                Trace.WriteLine(string.Format("TCP: Listening on {0}:{1}...", addr, port));

                listener.Start();

                while (true)
                {
                    var socket = await listener.AcceptSocketAsync();
                    var newClient = new Client(socket);
                    newClient.OnDisconnected += OnClientDisconnected;

                    //WorkerRole.ClientManager.GlobalClientList.Add(newClient);
                    newClient.ReceiveAsync();
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine(string.Format("TCP: Caught exception!! '{0}'\n{1}", e.Message, e.StackTrace));
                Trace.WriteLine("TcpListener shutting down.");
            }
        }

        private static void OnClientDisconnected(object sender, EventArgs eventArgs)
        {
            Trace.WriteLine("Client Disconnected.  Disposing client object.");
            var client = sender as Client;

            if (client != null)
            {
                client.Dispose();
            }
        }
    }
}
