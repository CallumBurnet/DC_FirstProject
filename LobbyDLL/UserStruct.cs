using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LobbyDLL
{
    internal class UserStruct
    {
        public int Id;
        public string Username;
        public uint currentLobbyId;
        public UserStruct() { 
            Id = 0;
            Username = "";
            currentLobbyId = 0;
        }
    }
}
