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
        // Using hashmaps, because we expect to search more than we edit
        // And because our removals require searches, too...
        private readonly Lobby lobby;
        private readonly string username;

        public LobbyServer(string username)
        {
            lobby = Lobby.GetInstance();  // Grab the internal singleton ref
            lobby.Join(username, this);  // May UnauthorisedUserFault
            this.username = username;
        }

        public void Leave(string username)
        {
            lobby.Leave(username);
            // Extra disconnect logic should go here, though I think they would already be deauthorised...
        }

        public void MakeRoom(string roomName, string owner)
        {
            lobby.MakeRoom(roomName, owner);
        }

        public void FetchRoomData(out List<string> roomNames, out List<uint> userCounts)
        {
            lobby.FetchRoomData(out roomNames, out userCounts);
        }
    }
}
