using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace WorkerRole
{
    /// <summary>
    /// Describes whether the incoming TCP connection is an external client or an internal server
    /// instance.
    /// </summary>
    public enum EndpointType
    {
        Internal,
        External
    }

    public class TcpListenerPropertyBag
    {
        public IPEndPoint endpoint;
        public EndpointType endpointType;
    }

    public class TCPListener
    {
        /// <summary>
        /// TCP thread that listens for incoming connections.
        /// </summary>
        public static async void TcpListenerProcess(object endpointObject)
        {
            try
            {
                var bag = endpointObject as TcpListenerPropertyBag;

                Trace.WriteLine(String.Format(
                        "TCP: Starting TcpListener of EndpointType {0}",
                        Enum.GetName(typeof(EndpointType), bag.endpointType)));

                TcpListener listener = null;

                //ToDo: Remove this 4027 endpoint code once this is uploaded for real.
                if (bag.endpointType == EndpointType.Internal)
                {
                    listener = new TcpListener(bag.endpoint) { ExclusiveAddressUse = false };
                    Trace.WriteLine(string.Format("TCP: Listening on {0}", bag.endpoint.Address.ToString()));
                }
                else
                {
                    const int port = 4032;
                    IPAddress addr = IPAddress.Loopback;
                    listener = new TcpListener(addr, port);
                    Trace.WriteLine(string.Format("TCP: Listening on {0}:{1}...", addr, port));
                }

                //const int port = 4027;
                //IPAddress addr = IPAddress.Loopback;
                //var listener = new TcpListener(addr, port);
                //Trace.WriteLine(string.Format("TCP: Listening on {0}:{1}...", addr, port));

                listener.Start();

                while (true)
                {
                    var socket = await listener.AcceptSocketAsync();

                    Client newClient = null;

                    switch (bag.endpointType)
                    {
                        case EndpointType.External:
                            newClient = new ExternalClient(socket);
                            break;

                        case EndpointType.Internal:
                            newClient = new InternalClient(socket);
                            WorkerRole.InternalClients.Add(newClient as InternalClient);
                            break;
                    }

                    if (newClient != null)
                    {
                        newClient.ReceiveAsync();
                    }
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
