using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;

namespace LobbyDLL
{
    [DataContract]
    public class UnauthorisedUserFault
    {
        [DataMember]
        public string problemType { get; set; }


        public UnauthorisedUserFault() { }
        public UnauthorisedUserFault(string problemType) { this.problemType = problemType; }
        public void ProblemType(string problemType)
        {
            this.problemType = problemType;
        }
    }
}
