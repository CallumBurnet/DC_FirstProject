using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace LobbyDLL
{
    internal class Room
    {
        public string name;
        public string owner;
        public HashSet<string> users;


        public Room(string name, string owner)
        {
            this.name= name;
            this.owner = owner;
            users = new HashSet<string>();
        }

        public void addUser(string userName)
        {
            users.Add(userName);
        }
        public void removeUser(string userName)
        {
            users.Remove(userName);
        }
        public HashSet<string> Users()
        {
            HashSet<string> clonedUsers = users.ToHashSet();
            return clonedUsers;
        }


    }
}
