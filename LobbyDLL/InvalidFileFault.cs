
using System.Runtime.Serialization;


namespace LobbyDLL
{
    [DataContract]
    public class InvalidFileFault
    {
        [DataMember]
        public string FileName { get; set; }   
    }
}
