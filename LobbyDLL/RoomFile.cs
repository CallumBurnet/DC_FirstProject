using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace LobbyDLL
{
    internal class RoomFile
    {
        public string name;
        public string extension;
        public DateTime timeUploaded;

        public RoomFile(string name, string extension, DateTime timeUploaded)
        {
            this.name = name;
            this.extension = extension;
            this.timeUploaded = timeUploaded;
        }
        public string Name() { return name; }
        public string Extension() { return extension; }
        public DateTime TimeUploaded() { return timeUploaded; }
    }
}
