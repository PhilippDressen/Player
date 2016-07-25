using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.IO;

namespace Player
{
    public class Track
    {
        public String Title { get; set; }

        public String Performer { get; set; }

        public String Album { get; set; }

        public String Path { get; set; }

        public DateTime Year { get; set; }

        public TimeSpan Length { get; set; }

        public BitmapImage AlbumArt { get; set; }

        public static Track ParseFromTaglib(TagLib.Tag t)
        {
            Track tag = new Track();
            tag.Title = t.Title;
            tag.Performer = t.FirstPerformer;
            tag.Album = t.Album;
            tag.Path = t.Comment;
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
            else
            {
                MemoryStream ms = new MemoryStream();
                Player.Properties.Resources.icon.Save(ms);
                BitmapImage Cover = new BitmapImage();
                Cover.BeginInit();
                Cover.StreamSource = ms;
                Cover.EndInit();
                tag.AlbumArt = Cover;
            }
            tag.AlbumArt.Freeze();
            return tag;
        }

        public static Track ParseFromMP3(string path)
        {
            if (!File.Exists(path))
                return null;

            Track tag = new Track();
            tag.Path = path;

            if (System.IO.Path.GetExtension(path) == ".mp3")
            {
                TagLib.File tagfile = TagLib.File.Create(path);
                TagLib.Tag t = tagfile.Tag;
                tagfile.Dispose();
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
                else
                {

                    MemoryStream ms = new MemoryStream();
                    Player.Properties.Resources.icon.Save(ms);
                    BitmapImage Cover = new BitmapImage();
                    Cover.BeginInit();
                    Cover.StreamSource = ms;
                    Cover.EndInit();
                    tag.AlbumArt = Cover;
                }
            }
            else
            {
                tag.Title = System.IO.Path.GetFileNameWithoutExtension(path);
                tag.Album = System.IO.Path.GetDirectoryName(path);
                MemoryStream ms = new MemoryStream();
                Player.Properties.Resources.icon.Save(ms);
                BitmapImage Cover = new BitmapImage();
                Cover.BeginInit();
                Cover.StreamSource = ms;
                Cover.EndInit();
                tag.AlbumArt = Cover;
            }
            tag.AlbumArt.Freeze();
            return tag;
        }        
    }
}
