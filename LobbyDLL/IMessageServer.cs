using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;

namespace LobbyDLL
{
    [ServiceContract(CallbackContract = typeof(IRoomServerCallback))]

    public interface IMessageServer
    {
        [OperationContract]
        [FaultContract(typeof(InvalidRoomFault))]
        void Join(string roomName, string username);
        [OperationContract]
        void Leave();


        [OperationContract(IsOneWay = true)]
        [FaultContract(typeof(DuplicateConnectionFault))]

        void SendPrivateMessage(string username, string from, string to);
        [OperationContract(IsOneWay = true)]
        [FaultContract(typeof(DuplicateConnectionFault))]
        void SendPublicMessage(string username, string from);
    }
    public interface IRoomServerCallback
    {
        [OperationContract(IsOneWay = true)]
        void PushMessage(string message);
    }
}
