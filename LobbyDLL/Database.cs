using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LobbyDLL
{
    public class Database
    {
        private List<UserStruct> _UserTable;
        private List<LobbyStruct> _LobbyTable;
        private List<MessageStruct> _MessageTable;
        private Database()
        {
            _UserTable = new List<UserStruct>();
            _LobbyTable = new List<LobbyStruct>();
            _MessageTable = new List<MessageStruct>();
        }
        public void addUser(string username)
        {
            int id = _UserTable.Count;
            var temp = new UserStruct();
            temp.Username = username;
            temp.Id = id;
            _UserTable.Add(temp);
        }

    }
}
