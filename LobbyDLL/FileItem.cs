using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Runtime.Serialization;

namespace LobbyDLL
{
    [DataContract]
    [KnownType(typeof(ImageFileItem))]
    [KnownType(typeof(TextFileItem))]
    public abstract class FileItem
    {
        [DataMember]
        public string fileName { get; set; }
    }
    [DataContract]
    public class ImageFileItem : FileItem {
        [DataMember]
        public Bitmap Bitmap { get; set; }
    }
    [DataContract]
    public class  TextFileItem : FileItem
    {
        [DataMember]
        public string TextContent { get; set; }
    }

}
