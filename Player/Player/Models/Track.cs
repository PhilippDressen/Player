using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.IO;

namespace Player.Models
{
    class Track
    {
        public Track()
        {
            Id = Guid.NewGuid();
        }

        bool _hastags = false;
        public bool HasTags { get { return _hastags; } }

        public Guid Id { get; }

        public string Title { get; set; }

        public string Performer { get; set; }

        public string Album { get; set; }

        public string Path { get; set; }

        public DateTime Year { get; set; }

        public TimeSpan Length { get; set; }

        public BitmapImage AlbumArt { get; set; }

        public static Track ParseFromTaglib(TagLib.Tag t)
        {
            Track tag = new Track();
            tag.Title = t.Title;
            tag.Performer = t.FirstPerformer;
            tag.Album = t.Album;
            if (t.Year > 0)
                tag.Year = new DateTime(Convert.ToInt32(t.Year), 1, 1);
            if (t.Pictures.Count() > 0)
            {
                MemoryStream ms = new MemoryStream(t.Pictures[0].Data.Data);
                BitmapImage Cover = new BitmapImage();
                Cover.BeginInit();
                Cover.StreamSource = ms;
                Cover.EndInit();
                tag.AlbumArt = Cover;
                tag.AlbumArt.Freeze();
            }
            return tag;
        }

        public static Track ParseFromFile(string path)
        {
            if (!File.Exists(path))
                throw new System.IO.FileLoadException("Die Datei ist nicht vorhanden");
            Track tag = new Track();
            if (System.IO.Path.GetExtension(path) != ".m3u")
            {
                tag.Path = path;

                TagLib.File tagfile = TagLib.File.Create(path);
                TagLib.Tag t = tagfile.Tag;
                tagfile.Dispose();
                if (string.IsNullOrEmpty(t.Title))
                    tag.Title = System.IO.Path.GetFileNameWithoutExtension(path);
                else
                    tag.Title = t.Title;
                tag.Performer = t.FirstPerformer;
                tag.Album = t.Album;
                if (t.Year > 0)
                    tag.Year = new DateTime(Convert.ToInt32(t.Year), 1, 1);
                if (t.Pictures.Count() > 0)
                {
                    MemoryStream ms = new MemoryStream(t.Pictures[0].Data.Data);
                    BitmapImage Cover = new BitmapImage();
                    Cover.BeginInit();
                    Cover.StreamSource = ms;
                    Cover.EndInit();
                    tag.AlbumArt = Cover;
                }
                if (tag.AlbumArt != null)
                    tag.AlbumArt.Freeze();
                tag._hastags = true;
            }
            else
            {
                tag.Path = path;
                tag.Title = System.IO.Path.GetFileNameWithoutExtension(path);
                tag.Album = System.IO.Path.GetDirectoryName(path);
            }
            return tag;
        }

        public override string ToString()
        {
            return string.Format("{0} von {1}", Title, Performer);
        }
    }
}
