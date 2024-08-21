using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.Runtime.Serialization;

namespace LobbyInterface
{
    [DataContract]
    public class UnauthorisedUserFault
    {
        [DataMember]
        public string UserName { get; set; } 
    }
}
