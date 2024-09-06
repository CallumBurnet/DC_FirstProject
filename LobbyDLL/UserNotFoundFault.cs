using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace LobbyDLL
{
    [DataContract]
    public  class UserNotFoundFault
    {
        [DataMember]
        public string problemType { get; set; }


        public UserNotFoundFault() { }
        public UserNotFoundFault(string problemType) { this.problemType = problemType; }
        public void ProblemType(string problemType)
        {
            this.problemType = problemType;
        }
    }
}
