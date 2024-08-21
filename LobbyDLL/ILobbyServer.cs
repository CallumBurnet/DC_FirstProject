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
        void JoinRoom(string roomName, string username);
        [OperationContract]
        void LeaveRoom(String roomName, String username);
        [OperationContract]
        void MakeRoom(String roomName, String owner);
        [OperationContract]
        void FetchRooms(out List<string> roomNames, out List<uint> userCounts);
        [OperationContract]
        HashSet<string> FetchRoomUsers(string roomNames, string username);

        [OperationContract]
        void AddFile(string roomName, File file, string username);
        void RelayFileChanges(string roomName, File file);
        [OperationContract]
        HashSet<string> FetchFileNames(string roomName, string username);
        [OperationContract]
        File FetchFile(string roomName, string filename, string username);
        [OperationContract]
        void SendPrivateMessage(string roomName, string from, string to);
        [OperationContract]
        void SendPublicMessage(string roomName, string from);
        void RelayMessage(string message, HashSet<string> targets);
        
    }
}
