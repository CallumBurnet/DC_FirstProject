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
        private Room room;
        private string username;

        public MessageServer()
        {
            room = null;
            username = "";
        }

        // Doesn't seem like it's possible to pass constructor params, so use a manual join and guards
        public void Join(string roomName, string username)
        {
            // Required setup, as other methods will throw on missing room
            room = Lobby.GetInstance().FetchRoom(roomName);  // May RoomNotFoundFault
            room.Join(username, this);  // May UnauthorisedUserFault or DuplicateConnectionFault
            this.username = username;
        }

        // All these just forward the internal logic and should be one-way
        public void Leave()
        {
            room.Leave(username);
            // Extra disconnect logic should go here, though I think they would already be deauthorised...
        }

        public List<string> FetchRoomUsers()
        {
            if (room == null || username == "")
            {
                UnauthorisedUserFault fault = new UnauthorisedUserFault();
                fault.ProblemType = "User not in room.";
                throw new FaultException<UnauthorisedUserFault>(fault, new FaultReason("User not in room."));
            }

            // For user reference when private messaging
            return room.Users();
        }

        public void SendPrivateMessage(string message, string from, string to)
        {
            if (room == null || username == "")
            {
                UnauthorisedUserFault fault = new UnauthorisedUserFault();
                fault.ProblemType = "User not in room.";
                throw new FaultException<UnauthorisedUserFault>(fault, new FaultReason("User not in room."));
            }

            room.SendPrivateMessage(message, from, to);  // May UserNotFoundFault
        }

        public void SendPublicMessage(string message, string from)
        {
            if (room == null || username == "")
            {
                UnauthorisedUserFault fault = new UnauthorisedUserFault();
                fault.ProblemType = "User not in room.";
                throw new FaultException<UnauthorisedUserFault>(fault, new FaultReason("User not in room."));
            }

            room.SendPublicMessage(message, from);
        }

        internal void RelayMessage(string message)
        {
            OperationContext.Current.GetCallbackChannel<IMessageServerCallback>.PushMessage(message);
        }
    }
}
