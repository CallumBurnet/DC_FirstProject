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
        [FaultContract(typeof(InvalidFileFault))]
        void AddFile(File file);


        [OperationContract] //Client Implementation of Callback ~ delegate example
        HashSet<string> FetchFileNames(string username);
        [OperationContract]
        File FetchFile(string username, string filename);
    }
    public interface IFileServerCallback
    {
        [OperationContract(IsOneWay = true)]
        void FileChanged();


    }
}
