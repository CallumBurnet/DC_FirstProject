using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace LobbyServer
{
    internal class MessageServer : IMessageServer
    {
        private readonly Room room;
        private readonly string username;

        public MessageServer(string roomName, string username)
        {
            room = Lobby.GetInstance().FetchRoom(roomName);  // May RoomNotFoundFault
            room.Join(username, this);  // May UnauthorisedUserFault or DuplicateConnectionFault
            this.username = username;
        }

        public List<string> FetchRoomUsers()
        {
            // For user reference when private messaging
            return room.Users();
        }

        // All these just forward the internal logic and should be one-way
        public void Leave()
        {
            room.Leave(username);
            // Extra disconnect logic should go here, though I think they would already be deauthorised...
        }

        public void SendPrivateMessage(string message, string from, string to)
        {
            room.SendPrivateMessage(message, from, to);  // May UnauthorisedUserFault or UserNotFoundFault
        }

        public void SendPublicMessage(string message, string from)
        {
            room.SendPublicMessage(message, from);  // May UnauthorisedUserFault
        }

        internal void RelayMessage(string message)
        {
            OperationContext.Current.GetCallbackChannel<IMessageServerCallback>.PushMessage(message);
        }
    }
}
