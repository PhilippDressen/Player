using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml.Linq;
using Player.Helpers;
using System.Collections.ObjectModel;

namespace Player.Models
{
    public class Playlist
    {
        public string Path { get; set; }

        public string Name
        { get; private set; }

        public int Length
        {
            get
            {
                return Tracks.Count;
            }
        }

        public ObservableCollection<Track> Tracks
        { get; }

        public Playlist(string path = null, string name = null)
        {
            this.Name = name;
            this.Path = path;
            this.Tracks = new ObservableCollection<Track>();            
        }

        public override string ToString()
        {
            return string.Format("{0} ({1})", Name, Length);
        }

        public static Playlist Load(string path)
        {
            Playlist pl = new Playlist(path);
            XDocument xd = XDocument.Load(path);
            if (xd.Element("smil")?.Element("head")?.Element("title")?.Value != null)
            {
                pl.Name = xd.Element("smil")?.Element("head")?.Element("title").Value;
            }

            if (xd.Element("smil")?.Element("body")?.Element("seq") != null)
            {
                foreach (XElement xe in xd.Element("smil")?.Element("body")?.Element("seq").Elements("media"))
                {
                    if (xe.Attribute("src") != null)
                    {
                        pl.Tracks.Add(new Track(xe.Attribute("src").Value));
                    }
                }                
            }

            return pl;
        }

        public void Save(string path)
        {
            Directory.CreateDirectory(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), "Wiedergabelisten"));

            XDocument xd = new XDocument();
            xd.AddFirst(new XElement("smil"));
            XElement head = new XElement("head");
            head.Add(
                new XElement("meta",
                    new XAttribute("name", "Generator"),
                    new XAttribute("content", System.Reflection.Assembly.GetExecutingAssembly().GetName())
                    ),
                    new XElement("meta",
                    new XAttribute("name", "ItemCount"),
                    new XAttribute("content", Tracks.Count())
                    ),
                    new XElement("title", System.IO.Path.GetFileNameWithoutExtension(path))
                );
            xd.Root.Add(head);
            XElement seq = new XElement("seq");
            foreach (Track t in Tracks)
            {
                seq.Add(
                    new XElement("media",
                        new XAttribute("src", t.Path)
                    )
                    );
            }
            xd.Root.Add(new XElement("body", seq));
            xd.Save(path);
        }
        
    }
}
