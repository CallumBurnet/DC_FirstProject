using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;  // For networking and contracts

using LobbyDLL;

namespace LobbyServer
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
    internal class LobbyServer : ILobbyServer
    {
        private readonly Lobby lobby;
        private string username;
        private int counter;
        public LobbyServer()
        {
            lobby = Lobby.GetInstance();  // Grab the internal singleton ref
            username = "";
        }

        // Doesn't seem like it's possible to pass constructor params, so use a manual join and guards
        public void JoinLobby(string username)
        {
            // Required setup, as other methods will throw unauthorised if not done
            lobby.Join(username, this);  // May UnauthorisedUserFault if duplicate
            this.username = username;
            
        }
        

        public void LeaveLobby()
        {
            lobby.Leave(username);
            // Extra disconnect logic should go here, though I think they would already be deauthorised...
        }

        public void MakeRoom(string roomName)
        {
            if (!lobby.ValidateUser(username))
            {
                UnauthorisedUserFault fault = new UnauthorisedUserFault();
                fault.problemType = "User not in lobby.";
                throw new FaultException<UnauthorisedUserFault>(fault, new FaultReason("User not in lobby."));
            }

            lobby.MakeRoom(roomName, username);  // May InvalidRoomFault
        }

        public void FetchRoomData(out List<string> roomNames, out List<uint> userCounts)
        {
            if (!lobby.ValidateUser(username))
            {
                UnauthorisedUserFault fault = new UnauthorisedUserFault();
                fault.problemType = "User not in lobby.";
                throw new FaultException<UnauthorisedUserFault>(fault, new FaultReason("User not in lobby."));
            }

            lobby.FetchRoomData(out roomNames, out userCounts);
        }
    }
}
