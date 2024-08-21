using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace LobbyInterface
{
    [DataContract]
    public class InvalidRoomFault
    {
        [DataMember]   
        public string Message { get; set; } 
    }
}
