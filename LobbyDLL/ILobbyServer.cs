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
        [FaultContract(typeof(UnauthorisedUserFault))]
        void JoinLobby(string username);

        [OperationContract]
        void LeaveLobby();

        [OperationContract]
        [FaultContract(typeof(InvalidRoomFault))]   
        void MakeRoom(string roomName);
        [OperationContract]
        // [FaultContract(typeof(void))]
        void FetchRoomData(out List<string> roomNames, out List<uint> userCounts, out List<string> users);
        [OperationContract]
        string getUserName();
     



    }
}
