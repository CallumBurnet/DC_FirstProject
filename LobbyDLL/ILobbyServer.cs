using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;

namespace LobbyDLL
{
    [ServiceContract]
    public interface ILobbyServer
    {
        [OperationContract]
        void JoinLobby(string username);

        [OperationContract]
        void LeaveLobby(string username);

        [OperationContract]
        [FaultContract(typeof(InvalidRoomFault))]   
        void MakeRoom(String roomName, String owner);
        [OperationContract]
        void FetchRooms(out List<string> roomNames, out List<uint> userCounts);
        [OperationContract]
        HashSet<string> FetchRoomUsers(string roomNames, string username);
        
    }
}
