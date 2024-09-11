using System;
using System.Runtime.Serialization;

namespace LobbyDLL
{
    [DataContract]
    public class RoomFile
    {
        [DataMember]
        public string name { get; set; }

        [DataMember]
        public string extension { get; set; }

        [DataMember]
        public DateTime? TimeUploaded { get; set; }

        [DataMember]
        public FileItem file { get; set; }

        // Parameterless constructor required for WCF serialization
        public RoomFile() { }

        // Constructor with parameters
        public RoomFile(string name, string extension, DateTime? timeUploaded, FileItem file)
        {
            this.name = name;
            this.extension = extension;
            this.TimeUploaded = timeUploaded;
            this.file = file;
        }
    }
}
