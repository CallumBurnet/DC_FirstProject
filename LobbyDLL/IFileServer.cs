using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.IO;
using LobbyDLL;
using System.Threading;

namespace LobbyDLL
{
    [ServiceContract(CallbackContract =typeof(IFileServerCallback))]
    public interface IFileServer
    {
        [OperationContract]
        [FaultContract(typeof(InvalidRoomFault))]
        [FaultContract(typeof(UnauthorisedUserFault))]
        [FaultContract(typeof(DuplicateConnectionFault))]
        void Join(string roomName, string userName);
        [OperationContract]
        void Leave();

        [OperationContract]
        [FaultContract(typeof(UnauthorisedUserFault))]
        [FaultContract(typeof(InvalidFileFault))]
        void AddFile(RoomFile file);

        [OperationContract]
        [FaultContract(typeof(UnauthorisedUserFault))]
        [FaultContract(typeof(InvalidFileFault))]
        List<string> FetchFileNames();
        [OperationContract]
        [FaultContract(typeof(UnauthorisedUserFault))]
        [FaultContract(typeof(InvalidFileFault))]
        RoomFile FetchFile(string fileName);
    }
    public interface IFileServerCallback
    {
        [OperationContract(IsOneWay = true)]
        void FileChanged();
       

    }
}
