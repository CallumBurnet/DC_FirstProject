using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace LobbyInterface
{
    [DataContract]
    public class InvalidMessageFault
    {
        [DataMember]
        public string Message { get; set; }
    }
}
