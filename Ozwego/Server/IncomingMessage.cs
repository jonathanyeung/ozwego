namespace Ozwego.Server
{
    public class IncomingMessage : Message
    {
        /// <summary>
        /// Delegates message processing to ProcessMessage strategy.
        /// </summary>
        private MessageProcessor ProcessMessage { get; set; }


        /// <summary>
        /// Setter for the MessageProcessor delegate.
        /// </summary>
        /// <param name="messageProcessor"></param>
        public void SetMessageProcessor(MessageProcessor messageProcessor)
        {
            ProcessMessage = messageProcessor;
        }


        /// <summary>
        /// Public method that is called to perform all of the necessary actions required on a 
        /// message received.
        /// </summary>
        public void HandleMessage()
        {
            ProcessMessage.DoActions(PacketType, MessageString, SenderEmailAddress);
        }
    }
}
