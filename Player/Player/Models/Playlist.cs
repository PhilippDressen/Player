using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml.Linq;
using Player.Helpers;

namespace Player.Models
{
    public class Playlist
    {
        public Playlist(string path)
        {
            Path = path;
            if (File.Exists(Path))
            {
                XDocument xd = XDocument.Load(Path);
                Length = xd.Root.Element("body").Element("seq").Elements("media").Count();
            }
        }

        public string Name
        {
            get
            {
                return System.IO.Path.GetFileNameWithoutExtension(Path);
            }
        }

        public string Path { get; set; }

        public int Length
        {
            get;
            set;
        }

        public static Playlist Create(string path)
        {
            Playlist p = new Playlist(path);
            return p;
        }

        public override string ToString()
        {
            return Name;
        }

        public static Playlist SaveToPlaylist(IEnumerable<Track> tracks, string path)
        {
            Directory.CreateDirectory(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), "Wiedergabelisten"));
            //try
            //{
            //    Directory.CreateDirectory(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), "Wiedergabelisten"));
            //}
            //catch (Exception ex)
            //{
            //    Log.Write(ex.ToString(), EventType.Error);
            //    App.Current.Dispatcher.Invoke(() => Info.Show("Playlistordner konnte nicht erstellt werden!"));
            //}

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
                    new XAttribute("content", tracks.Count())
                    ),
                    new XElement("title", System.IO.Path.GetFileNameWithoutExtension(path))
                );
            xd.Root.Add(head);
            XElement seq = new XElement("seq");
            foreach (Track t in tracks)
            {
                seq.Add(
                    new XElement("media",
                        new XAttribute("src", t.Path)
                    )
                    );
            }
            xd.Root.Add(new XElement("body", seq));
            xd.Save(path);
            return new Playlist(path);
        }
    }
}
