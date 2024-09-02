using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LobbyDLL
{
    internal class FileStruct
    {
        public uint fileId;
        public string lobbyName;
        public string fileName;
        public string fileType;
        public FileStruct()
        {
            fileId = 0;
            lobbyName = "";
            fileName = "";
            fileType = "";
        }
    }
}
