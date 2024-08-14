using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LobbyDLL
{
    internal class MessageStruct
    {
        public uint messageId;
        public uint sourceUser;
        public uint destinationUser;
        public Boolean isPrivate;
        public string messageText;
        public MessageStruct()
        {
            messageId = 0;
            sourceUser = 0;
            destinationUser = 0;
            isPrivate = false;
            messageText = "";
        }
    }
}
