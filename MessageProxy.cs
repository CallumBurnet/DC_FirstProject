using LobbyDLL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace LobbyClient
{
    public class MessageProxy : IRoomServerCallback //message proxy is also the implementation of the message callback
    {
        private IMessageServer _server;
        private string userName;
        public MessageProxy(IMessageServer server, string userName = null)
        {
            _server = server;
            this.userName = userName;
        }
        public void SendPublic(string message)
        {
            _server.SendPublicMessage(message, this.userName);
        }
        public void PushMessage(string message)
        {
            
        }

}
}
