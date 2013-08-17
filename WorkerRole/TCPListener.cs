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

            const int port = 4029;
            //const int port = 10100;
            IPAddress addr = IPAddress.Loopback;
            //IPAddress addr = NetworkUtils.GetActiveIPv4Address();
            //var addr = new IPAddress(new byte[] {192, 168, 1, 2});

            try
            {
                var listener = new TcpListener(
                    RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["Endpoint1"].IPEndpoint);

                listener.ExclusiveAddressUse = false; 

                //var listener = new TcpListener(addr, port);
                listener.Start();
                Trace.WriteLine(string.Format("TCP: Listening on {0}:{1}...", addr, port));

                while (true)
                {
                    var socket = await listener.AcceptSocketAsync();
                    var newClient = new Client(socket);

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
    }
}
