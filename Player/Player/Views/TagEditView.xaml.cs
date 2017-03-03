using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Media.Animation;
using TagLib.Id3v2;
using System.IO;
using Player.Models;
using Player.Helpers;

namespace Player.Views
{
    /// <summary>
    /// Interaktionslogik für TagEditView.xaml
    /// </summary>
    public partial class TagEditView : Window
    {
        public TagLib.File TagFile;
        public Track TagTrack;

        public TagEditView()
        {
            InitializeComponent();
        }

        public static void Show(Window owner, Track t)
        {
            TagEditView tev = new TagEditView();
            tev.Owner = owner;
            tev.Show();
            new Thread(() => tev.Rescan(t)).Start();
        }

        private void Rescan(Track t)
        {
            if (t == null)
                return;

            try
            {
                TagTrack = t;
                TagFile = TagLib.File.Create(t.Path);
                TagLib.Tag tag = TagFile.Tag;
                Dispatcher.Invoke(() => Title = "TagEdit - " + System.IO.Path.GetFileName(TagFile.Name));
                Dispatcher.Invoke(() => ToolTip = TagFile.Name);
                Dispatcher.Invoke(() => tb_title.Text = tag.Title);
                string perfs = tag.FirstPerformer;
                foreach (string s in tag.Performers.Skip(1))
                {
                    perfs += "; " + s;
                }
                Dispatcher.Invoke(() =>  tb_performer.Text = perfs);
                Dispatcher.Invoke(() => tb_album.Text = tag.Album);
                Dispatcher.Invoke(() => tb_jahr.Text = tag.Year.ToString());
                if (tag.Pictures.Length > 0)
                {
                    MemoryStream ms = new MemoryStream(tag.Pictures[0].Data.Data);
                    BitmapImage bi = new BitmapImage();
                    bi.BeginInit();
                    bi.StreamSource = ms;
                    bi.EndInit();
                    bi.Freeze();
                    Dispatcher.Invoke(() =>
                    {
                        i_cover.Source = bi;
                        DoubleAnimation da = new DoubleAnimation(1, TimeSpan.FromSeconds(1));
                        i_cover.BeginAnimation(Image.OpacityProperty, da);
                    });
                }                
            }
            catch (Exception ex)
            {
                Log.Write("Fehler beim Laden in TagEdit: " + Environment.NewLine + ex.ToString(), EventType.Error);
                Dispatcher.Invoke(() => Info.Show("Fehler!" + Environment.NewLine + ex.Message));
                Dispatcher.Invoke(Close);
            }
        }

        private void mi_addcover_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.Title = "Bilddatei laden...";
            ofd.Filter = "Bild-Dateien|*.jpg; *.png";
            ofd.ShowDialog();

            if (File.Exists(ofd.FileName))
            {
                new Thread(() =>
                {
                    try
                    {
                        TagLib.Id3v2.AttachedPictureFrame pic = new TagLib.Id3v2.AttachedPictureFrame();
                        pic.TextEncoding = TagLib.StringType.Latin1;
                        pic.MimeType = System.Net.Mime.MediaTypeNames.Image.Jpeg;
                        pic.Type = TagLib.PictureType.FrontCover;
                        pic.Data = TagLib.ByteVector.FromPath(ofd.FileName);

                        TagFile.Tag.Pictures = new TagLib.IPicture[1] { pic };
                        TagFile.Save();

                        Dispatcher.Invoke(() =>
                        {
                            i_cover.Source = new BitmapImage(new Uri(ofd.FileName));
                        });
                    }
                    catch
                    {
                        MessageBox.Show("Es ist ein Fehler beim speichern aufgetreten. Ist die Datei in Verwendung?", "Speicherfehler!", MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                }).Start();

            }
            else
            {
                if (ofd.FileName == "")
                    return;
                MessageBox.Show("Die Datei \"" + ofd.FileName + "\" kann nicht geöffnet werden, da sie nicht existiert!", "Fehler!", MessageBoxButton.OK, MessageBoxImage.Error);
                IsEnabled = true;
            }
        }

        private void mi_bildspeichern_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog sfd = new Microsoft.Win32.SaveFileDialog();
            sfd.AddExtension = true;
            sfd.DefaultExt = ".jpg";
            sfd.Filter = "JPG-Bilder|*.jpg";
            sfd.FileName = TagFile.Tag.Title + " - cover";
            sfd.Title = "Coverbild speichern...";
            sfd.FileOk += Sfd_FileOk;
            sfd.ShowDialog();            
                           
        }

        private void Sfd_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            string FileName = (sender as Microsoft.Win32.SaveFileDialog).FileName;

            new Thread(() =>
            {
                try
                {
                    if (TagFile.Tag.Pictures.Count() > 0)
                    {
                        FileStream fs = new FileStream(FileName, FileMode.Create);
                        fs.Write(TagFile.Tag.Pictures[0].Data.Data, 0, TagFile.Tag.Pictures[0].Data.Data.Length);
                        fs.Close();
                    }

                }
                catch (Exception ex)
                {
                    Log.Write("Fehler beim Coverexport!" + Environment.NewLine + ex.ToString(), EventType.Error);
                    Dispatcher.Invoke(() => Info.Show("Fehler!" + Environment.NewLine + ex.Message));
                }
                Dispatcher.Invoke(() => IsEnabled = true);
            }).Start();
        }

        private void mi_deletecover_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Möchten sie das Coverbild wirklich löschen?", "Sicher?", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No) != MessageBoxResult.Yes)
                return;

            new Thread(() =>
            {
                try
                {
                    //TagLib.Id3v2.AttachedPictureFrame pic = new TagLib.Id3v2.AttachedPictureFrame();
                    //pic.TextEncoding = TagLib.StringType.Latin1;
                    //pic.MimeType = System.Net.Mime.MediaTypeNames.Image.Jpeg;
                    //pic.Type = TagLib.PictureType.FrontCover;
                    //pic.Data = TagLib.ByteVector.FromPath(ofd.FileName);

                    TagFile.Tag.Pictures = null; // new TagLib.IPicture[1] { pic };
                    TagFile.Save();

                    Dispatcher.Invoke(() =>
                    {
                        i_cover.Source = null;
                    });
                }
                catch
                {
                    MessageBox.Show("Es ist ein Fehler beim speichern aufgetreten. Ist die Datei in Verwendung?", "Speicherfehler!", MessageBoxButton.OK, MessageBoxImage.Error);
                }

            }).Start();
        }

        private void w_tagedit_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (i_cover.Source == null)
                mi_addcover.Visibility = Visibility.Visible;
            else
                mi_addcover.Visibility = Visibility.Collapsed;
        }

        private void Label_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //System.Diagnostics.Process.Start(TagFile.Name);
        }

        private void b_save_Click(object sender, RoutedEventArgs e)
        {
            IsEnabled = false;
            TagLib.Tag tag = TagFile.Tag;
            tag.Title = tb_title.Text;
            tag.Performers = tb_performer.Text.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            tag.Album = tb_album.Text;
            try
            {
                tag.Year = Convert.ToUInt32(tb_jahr.Text);
            }
            catch
            {
                MessageBox.Show("Das Jahr muss als uint (ganzzahliger positiver Wert) angegeben werden!");
                IsEnabled = true;
                return;
            }
            new Thread(Save).Start();            
        }

        private void Save()
        {
            try
            {                
                TagFile.Save();
                Dispatcher.Invoke(() =>
                    {
                        //if (TagTrack != null && Prop.View != null && Prop.Playlist.Contains(TagTrack))
                        //{
                        //    int ind = Prop.Playlist.IndexOf(TagTrack);
                        //    Prop.Playlist.Remove(TagTrack);
                        //    Track t = Track.ParseFromTaglib(TagFile.Tag);
                        //    t.Path = TagFile.Name;
                        //    Prop.Playlist.Insert(ind, t);
                        //}
                        // To Do: throw changed tags back to origin
                    });
            }
            catch (Exception ex)
            {
                Log.Write("Fehler beim Laden in TagEdit: " + Environment.NewLine + ex.ToString(), EventType.Error);
                Dispatcher.Invoke(() => Info.Show("Fehler!" + Environment.NewLine + ex.Message));
            }
            Dispatcher.Invoke(Close);
        }
    }
}
