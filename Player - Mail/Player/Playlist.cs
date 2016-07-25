using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Linq;

namespace Player
{
    class Playlist
    {
        public String Name
        {
            set { }
            get
            {
                return System.IO.Path.GetFileNameWithoutExtension(Path);
            }
        }

        public String Path { get; set; }

        public int Length
        {
            get
            {
                if (File.Exists(Path))
                {
                    XDocument xd = XDocument.Load(Path);
                    if (xd.Element("Playlist") != null)
                    {
                        return xd.Element("Playlist").Elements("Track").Count();
                    }
                }
                return 0;
            }
        }

        public static Playlist Create(string path)
        {
            Playlist p = new Playlist();
            p.Path = path;
            return p;
        }
    }
}
