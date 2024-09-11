using LobbyDLL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;


namespace LobbyClient
{
    public class MessageProxy : IRoomServerCallback //message proxy is also the implementation of the message callback
    {
        
        ChannelFactory<IMessageServer> messageFactory;
        private IMessageServer server;
        private string userName;
        private string roomName;
        
        public MessageProxy(string userName, string roomName)
        {
            string messageURL = "net.tcp://localhost:8100/message"; // Sets the endpoint
            NetTcpBinding binding = new NetTcpBinding();
            messageFactory = new DuplexChannelFactory<IMessageServer>(this, binding, new EndpointAddress("net.tcp://localhost:8100/message")); //message factory
            server = messageFactory.CreateChannel();
            this.userName = userName;
            this.roomName = roomName;
            server.Join(roomName, userName);
        }
        public void SendPublic(string message)
        {
            server.SendPublicMessage(message, this.userName);
        }
        public void PushMessage(string message)
        {
            
        }


    }
}
