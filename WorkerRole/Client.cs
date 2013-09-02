using System;
using System.Diagnostics;
using System.Net.Sockets;

namespace WorkerRole
{
    delegate void OnMessageReceivedHandler(Client client, byte[] msgBytes);

    /// <summary>
    /// Represents a client instance with a TCP connection to the server.
    /// </summary>
    public class Client
    {

        private readonly Socket _socket = null;
        private readonly int _socketId;
        public Room Room;

        public Client(Socket client)
        {
            _socket = client;

            //
            // Create a new room for the new connecting client.
            //
            
            Room = WorkerRole.RoomManager.CreateNewRoom(this);
        }

        public string UserName;


        ~Client()
        {
            if (_socket != null)
            {
                try
                {
                    Disconnect();
                    _socket.Dispose();
                }
                catch (Exception e)
                {
                    Trace.WriteLine(e.ToString());
                }
            }
        }


        public bool IsConnected
        {
            get
            {
                try
                {
                    //ToDo: Determine wtf this line of code is about:
                    return !(_socket.Poll(1, SelectMode.SelectRead) && 
                            _socket.Available == 0 || 
                            _socket.Connected == false);
                }
                catch
                {
                    return false;
                }
            }
        }

        #region Public Methods

        public void Disconnect()
        {
            try
            {
                if (IsConnected)
                {
                    _socket.Disconnect(false);
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine(string.Format("Disconnect: Caught exception!! '{0}'\n{1}", e.Message, e.StackTrace));
            }
            finally
            {
                WorkerRole.ClientManager.RemoveClient(this);
                _socket.Close();
            }
        }


        public void Send(byte[] data)
        {
            if (IsConnected)
            {
                var sendArgs = new SocketAsyncEventArgs();
                sendArgs.SetBuffer(data, 0, data.Length);
                sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>((obj, args) =>
                {
                    if (args.BytesTransferred == 0 || args.SocketError != SocketError.Success)
                    {
                        Trace.WriteLine("TCP: : Error, Failed to send data!");
                    }
                });

                _socket.SendAsync(sendArgs);
            }
        }

        #endregion

        /// <summary>
        /// This method asynchronously waits for incoming messages on the TCP socket. It
        /// recursively calls itself on the message received callback in order to 
        /// continue to listen for incoming packets.
        /// </summary>
        public void ReceiveAsync()
        {
            //
            // The first four bytes represent a uint containing the length of the sent message
            //

            var incomingMessageSizeArray = new byte[sizeof(uint)];


            //
            // This first call into the receive loop is just to get the uint length. In the
            // callback, a second loop is started to get the actual message bytes.
            //

            MessageReceiverLoop(incomingMessageSizeArray, 0, sizeof(uint), (_socket, _bufferArray) =>
            {
                //
                // These bytes represent the size of the message.
                //

                uint totalCount = BitConverter.ToUInt32(incomingMessageSizeArray, 0);
                incomingMessageSizeArray = new byte[totalCount];


                //
                // Now, this loop is to parse out the actual message.
                //

                MessageReceiverLoop(incomingMessageSizeArray, 0, totalCount, (clientSocket, msgBytes) =>
                {
                    WorkerRole.IncomingMessageHandler.HandleMessage(ref clientSocket, msgBytes);
                    ReceiveAsync();
                });
            });
        }


        /// <summary>
        /// This method waits for data to come in from the TCP socket. It recursively loops until
        /// the specified amount of data is received, after which it will call the callback
        /// delegate that is supplied.
        /// </summary>
        /// <param name="incomingMessageBuffer">
        /// The message buffer to copy the received message to</param>
        /// <param name="bufferOffset">
        /// The offset into the incoming TCP message stream to start reading data from</param>
        /// <param name="numberOfBytesToRead">
        /// The number of bytes to read before exiting the loop</param>
        /// <param name="messageReceivedCallback">
        /// Callback that runs once all the data has been received</param>
        /// <returns></returns>
        private bool MessageReceiverLoop(
            byte[] incomingMessageBuffer,
            int bufferOffset,
            uint numberOfBytesToRead,
            OnMessageReceivedHandler messageReceivedCallback)
        {
            //
            // Check and protect against bad parameters
            //

            if (incomingMessageBuffer == null || 
                numberOfBytesToRead > incomingMessageBuffer.Length || 
                bufferOffset >= incomingMessageBuffer.Length)
            {
                return false;
            }

            if (numberOfBytesToRead == 0)
            {
                if (messageReceivedCallback != null)
                {
                    messageReceivedCallback(this, incomingMessageBuffer);
                }
                return true;
            }

            //try
            //{
                _socket.BeginReceive(
                    incomingMessageBuffer, 
                    bufferOffset, 
                    incomingMessageBuffer.Length - bufferOffset, 
                    SocketFlags.None, 
                    aResult =>
                    {
                        //
                        // Callback once data is received through the socket.
                        //

                        //ToDo: Determine if we need to re-enable this code.
                        // Possible that our socket instance has changed between BeginReceive and EndReceive.
                        //int? thisSocketID = aResult.AsyncState as int?;
                        //if (!thisSocketID.HasValue || thisSocketID.Value != _socketID)
                        //{
                        //    ReceiveAsync();
                        //    return;
                        //}

                        try
                        {
                            int bytesReceived = _socket.EndReceive(aResult);

                            if (bytesReceived == 0 && !IsConnected)
                            {
                                Disconnect();
                                return;
                            }


                            //
                            // If all the bytes that are being waited on have been received,
                            // execute the callback.  Otherwise, recursively call this method
                            // again and wait to receive more data.
                            //

                            int receiveCount = bytesReceived;
                            if (receiveCount >= numberOfBytesToRead)
                            {
                                if (messageReceivedCallback != null)
                                {
                                    messageReceivedCallback(this, incomingMessageBuffer);
                                }
                            }
                            else
                            {
                                MessageReceiverLoop(
                                        incomingMessageBuffer, 
                                        receiveCount, 
                                        numberOfBytesToRead, 
                                        messageReceivedCallback);
                            }
                        }
                        catch (SocketException se)
                        {
                            if (!IsConnected)
                            {
                                Disconnect();
                                return;
                            }

                            Trace.WriteLine(string.Format("TCPReceiveLoop: Client {0} has closed the connection.", this.UserName));
                            return;
                        }
                        //ToDo: Re-enable this catch all block.
                        //catch (Exception e)
                        //{
                        //    if (!IsConnected)
                        //    {
                        //        Disconnect();
                        //        return;
                        //    }

                        //    Trace.WriteLine(string.Format("TCPReceiveLoop: Caught exception!! '{0}'\n{1}", e.Message, e.StackTrace));
                        //}
                    },
                    _socketId); // This used to be _socketID.  ToDo: Determine what the point of this object is.
            //}
            //ToDo: Re-enable this catch all block.
            //catch(Exception e)
            //{
            //    if (!IsConnected)
            //    {
            //        Disconnect();
            //        return false;
            //    }

            //    Trace.WriteLine(string.Format("TCPReceiveLoop: Caught exception!! '{0}'\n{1}", e.Message, e.StackTrace));
            //    return false;
            //}

            return true;
        }


        //private void OnDisconnected()
        //{
        //    // ToDo: Here we need to broadcast that this user has signed out.
        //    _socket.Close();
        //}
    }
}
