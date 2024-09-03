﻿using System;
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
        void Join(string roomName, string username);
        [OperationContract]
        void Leave();

        [OperationContract]
        [FaultContract(typeof(InvalidFileFault))]
        void AddFile(File file);

        [OperationContract] //Client Implementation of Callback ~ delegate example
        [FaultContract(typeof(InvalidFileFault))]
        HashSet<string> FetchFileNames(string username);
        [OperationContract]
        [FaultContract(typeof(InvalidFileFault))]
        File FetchFile(string username, string filename);
    }
    public interface IFileServerCallback
    {
        [OperationContract(IsOneWay = true)]
        void FileChanged();


    }
}
