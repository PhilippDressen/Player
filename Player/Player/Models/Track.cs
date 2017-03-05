using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.IO;
using Player.Helpers;

namespace Player.Models
{
    public class Track
    {
        public Track(string path = null)
        {
            this.Id = Guid.NewGuid();
            this.Path = path;
        }

        bool initialized = false;
        public bool Initialized { get { return initialized; } }

        public Guid Id { get; }

        public string Title { get; set; }

        public string Performer { get; set; }

        public string Album { get; set; }

        public string Path { get; private set; }

        public uint Year { get; set; }

        public TimeSpan Length { get; set; }

        public BitmapImage AlbumArt { get; set; }

        public override string ToString()
        {
            return string.Format("{0} von {1}", Title, Performer);
        }

        public void Initialize()
        {
            Reset();
            if (System.IO.Path.GetExtension(this.Path) != ".m3u")
            {
                TagLib.File tagfile = TagLib.File.Create(this.Path);
                TagLib.Tag t = tagfile.Tag;
                tagfile.Dispose();
                if (string.IsNullOrEmpty(t.Title))
                    this.Title = System.IO.Path.GetFileNameWithoutExtension(this.Path);
                else
                    this.Title = t.Title;
                this.Performer = t.FirstPerformer;
                this.Album = t.Album;
                if (t.Year > 0)
                    this.Year = Convert.ToUInt32(t.Year);
                if (t.Pictures.Count() > 0)
                {
                    MemoryStream ms = new MemoryStream(t.Pictures[0].Data.Data);
                    BitmapImage Cover = new BitmapImage();
                    Cover.BeginInit();
                    Cover.StreamSource = ms;
                    Cover.EndInit();
                    this.AlbumArt = Cover;
                }
                if (this.AlbumArt != null)
                    this.AlbumArt.Freeze();
            }
            else
            {
                this.Path = this.Path;
                this.Title = System.IO.Path.GetFileNameWithoutExtension(this.Path);
                this.Album = System.IO.Path.GetDirectoryName(this.Path);
            }
            this.initialized = true;
        }

        private void Reset()
        {
            Title = null;
            Performer = null;
            Album = null;
            Year = 0;
            Length = new TimeSpan();
            AlbumArt = null;
            initialized = false;
        }

        public static Track ParseFromTaglib(TagLib.Tag t)
        {
            Track tag = new Track();
            tag.Title = t.Title;
            tag.Performer = t.FirstPerformer;
            tag.Album = t.Album;
            if (t.Year > 0)
                tag.Year = Convert.ToUInt32(t.Year);
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
                    tag.Year = Convert.ToUInt32(t.Year);
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
            }
            else
            {
                tag.Path = path;
                tag.Title = System.IO.Path.GetFileNameWithoutExtension(path);
                tag.Album = System.IO.Path.GetDirectoryName(path);
            }
            tag.initialized = true;            
            return tag;
        }        
    }
}
