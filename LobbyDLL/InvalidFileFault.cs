
using System.Runtime.Serialization;


namespace LobbyDLL
{
    [DataContract]
    public class InvalidFileFault
    {
        [DataMember]
        public string problemType { get; set; }
        public InvalidFileFault() { }
        public InvalidFileFault(string problemType) { this.problemType = problemType; }

        public void ProblemType(string problemType)
        {
            this.problemType = problemType;
        }


    }
}
