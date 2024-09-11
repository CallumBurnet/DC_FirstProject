using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.IO;
using LobbyDLL;

namespace LobbyDLL
{
    [ServiceContract(CallbackContract =typeof(IFileServerCallback))]
    public interface IFileServer
    {
        [OperationContract]
        [FaultContract(typeof(InvalidRoomFault))]
        void Join(string roomName, string userName);
        [OperationContract]
        void Leave();

        [OperationContract]
        [FaultContract(typeof(InvalidFileFault))]
        void AddFile(RoomFile file);

        [OperationContract] //Client Implementation of Callback ~ delegate example
        [FaultContract(typeof(InvalidFileFault))]
        List<string> FetchFileNames();
        [OperationContract]
        [FaultContract(typeof(InvalidFileFault))]
        RoomFile FetchFile(string fileName);
    }
    public interface IFileServerCallback
    {
        [OperationContract(IsOneWay = true)]
        void FileChanged();
        [OperationContract(IsOneWay = true)]
        void DownloadProgress(string fileName, int percentage);

    }
}
