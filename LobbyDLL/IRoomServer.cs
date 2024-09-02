using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;

namespace LobbyDLL
{
    [ServiceContract(CallbackContract = typeof(IRoomServerCallback))]

    public interface IRoomServer
    {
        [OperationContract]
        void JoinRoom(string username);
        [OperationContract]
        void LeaveRoom(string username);


        [OperationContract(IsOneWay = true)]
        void SendPrivateMessage(string username, string from, string to);
        [OperationContract(IsOneWay = true)]
        void SendPublicMessage(string username, string from);
    }
    public interface IRoomServerCallback
    {
        [OperationContract(IsOneWay = true)]
        void PushMessage(string message);
    }
}
