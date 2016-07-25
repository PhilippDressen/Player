using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Windows.Threading;
using System.Threading;
using System.Collections.ObjectModel;
using System.Windows.Media.Animation;
using System.Diagnostics;
using System.Reflection;
using System.Xml.Linq;

namespace Player
{ 
    public partial class MainWindow : Window
    {
        #region Vars
        public bool Abbrechen
        {
            get
            {
                return threadabort;
            }
            set
            {
                threadabort = value;
            }
        }
        private bool MakeBool(string s)
        {
            if (s.ToLower() == "true")
            { return true; }
            else
            { return false; }
        }

        MediaPlayer mp = new MediaPlayer();
        Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
        Microsoft.Win32.OpenFileDialog oplst = new Microsoft.Win32.OpenFileDialog();
        DispatcherTimer timer = new DispatcherTimer();
        DispatcherTimer delay = new DispatcherTimer();
        ObservableCollection<Playlist> Playlists = new ObservableCollection<Playlist>();
        ObservableCollection<Track> Tracks = new ObservableCollection<Track>();
        Storyboard Start, Beenden;
        Laden laden;

        int track = 0, seconds, moveindex, geladen = 0, zuladen = 0;
        bool playing, wiederholen, positionmerken, settingsloaded, dragging, mute, key = true, animationcomplete = true, ausklappencomplete = true, scanning, threadabort, einstellungenladen = true,
            zufall, schwarz, cancleclosing;
        string info, path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Player", savefile, savedb, file,
            playlist, playlistordner;
        double speed;
        Key pressed = Key.None;

        private string PlaylistName()
        {
            string res;
            if (w_player.Dispatcher.CheckAccess())
            {
                NeuePlaylist dialog = new NeuePlaylist();
                if (this.IsLoaded)
                {
                    dialog.Owner = this;
                }
                dialog.ShowDialog();
                res = dialog.Result;
            }
            else
            {
                res = "";
                w_player.Dispatcher.Invoke(new ThreadStart(() => res = this.PlaylistName()));
            }
            return res;
        }

        private bool DialogView(string message)
        {
            bool res;
            if (w_player.Dispatcher.CheckAccess())
            {
                Dialog dialog = new Dialog();
                if (this.IsLoaded)
                {
                    dialog.Owner = this;
                }
                res = dialog.View(message);
            }
            else
            {
                res = false;
                w_player.Dispatcher.Invoke(new ThreadStart(() => res = this.DialogView(message)));
            }
            return res;
        }

        private void InfoView(string message)
        {
            if (w_player.Dispatcher.CheckAccess())
            {
                Info info = new Info();
                if (this.IsLoaded)
                {
                    info.Owner = this;
                }
                info.View(message);
            }
            else
            {
                Dispatcher.Invoke(new ThreadStart(() => this.InfoView(message)));
            }
        }
        #endregion Vars

        public MainWindow()
        {
            InitializeComponent();
            Start = TryFindResource("Start") as Storyboard;
            Start.Completed += Start_Completed;

            Beenden = TryFindResource("Beenden") as Storyboard;
            Beenden.Completed += Beenden_Completed;

            string[] startargs = Environment.GetCommandLineArgs();
            if (startargs.Count() > 1)
            {
                file = startargs[1];
            }
            savefile = path + @"\config.xml";
            savedb = path + @"\save.db";
            playlistordner = path + @"\playlists\";
            if (Directory.Exists(path + @"\playlists\") == false)
            {
                Directory.CreateDirectory(path + @"\playlists\");
            }

            ofd.FileOk += ofd_FileOk;
            ofd.Filter = "Audiodateien (*.mp3, *.wav, *.wmv, *.wma, *.m3u, *.mp4)|*.mp3; *.wav; *.wmv; *.wma; *.m3u; *.mp4"; //|"Alle Dateien (*.*)|*.*";
            ofd.Multiselect = true;

            oplst.Filter = "Playlistdateien (*.plst, *.zpl, *.wpl)|*.plst; *.zpl; *.wpl" ;
            oplst.Multiselect = false;
            oplst.FileOk += oplst_FileOk;

            mp.MediaEnded += mp_MediaEnded;
            mp.MediaFailed += mp_MediaFailed;
            mp.MediaOpened += mp_MediaOpened;

            timer.Interval = new TimeSpan(0, 0, 0, 0, 50);
            timer.Tick += timer_Tick;
            
            delay.Interval = new TimeSpan(0, 0, 2);
            delay.Tick += delay_Tick;
        }

        #region MediaPlayer
        void ofd_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StartScan(ofd.FileNames.ToList());
        }

        void mp_MediaOpened(object sender, EventArgs e)
        {
            Track tag = Tracks.ElementAt(track);
            string name = tag.Title, interpret = tag.Performer, album = tag.Album;            
            
            if (interpret == null)
            {
                interpret = "";
            }
            else
            {
                interpret = Environment.NewLine  + interpret;
            }
            if (album == null)
            {
                album = "";
            }
            else
            {
                album = Environment.NewLine + album;
            }
            info = name + interpret + album;
            if (tag.AlbumArt != null)
            {
                i_cover.Source = tag.AlbumArt;
                i_covermini.Source = tag.AlbumArt;                
            }
            else
            {
                i_cover.Source = w_player.Icon;
                i_covermini.Source = w_player.Icon;
                i_cover.Height = double.NaN;
                i_cover.Width = double.NaN;
                i_covermini.Height = double.NaN;
                i_covermini.Width = double.NaN;
            }
            b_play.IsEnabled = true;
            g_controls.IsEnabled = true;            
            timer.Start();
            mp.Volume = s_lautstärke.Value * 0.01;
            mp.IsMuted = mute;
            mp.SpeedRatio = speed;
            if (playing)
            {                
                b_play.Content = "II";
                mp.Play();
            }
            else
            {
                mp.Pause();
                b_play.Content = "►";                
            }            
            if (settingsloaded == false && mp.NaturalDuration.HasTimeSpan && seconds <= mp.NaturalDuration.TimeSpan.TotalSeconds)
            {
                mp.Position = new TimeSpan(0, 0, seconds);
                settingsloaded = true;
            }

            i_covermini.Width = l_info.ActualHeight;
            i_covermini.Height = l_info.ActualHeight;
            l_info.Margin = new Thickness(i_covermini.ActualWidth + 5, 0, 0, 0);
            pb_position.Margin = new Thickness(0, i_covermini.ActualWidth + 5, 0, 0);
        }

        void mp_MediaFailed(object sender, ExceptionEventArgs e)
        {
            Tracks.RemoveAt(track);
            b_play.IsEnabled = false;
            b_play.Content = "►";
            info = "";
            InfoView("Fehler bei der Wiedergabe!");
        }

        void mp_MediaEnded(object sender, EventArgs e)
        {
            Next();
        }

        private void PlayPause()
        {
            if (playing)
            {
                playing = false;
                mp.Pause();
                b_play.Content = "►";
            }
            else
            {
                if (b_play.IsEnabled)
                {
                    playing = true;
                    mp.Play();
                    b_play.Content = "II";
                }
            }
        }

        private void Next()
        {
            if (Tracks.Count - 1 > track)
            {
                if (zufall)
                {
                    Random r = new Random();
                    int lasttrack = track;
                    while (track == lasttrack)
                    {
                        track = r.Next(Tracks.Count - 1);
                    }
                }
                else
                {
                    track++;
                }
                if (playing)
                {
                    Play();
                }
                else
                {
                    if (Tracks.Count > track)
                    {
                        mp.Close();
                        mp.Open(new Uri(Tracks.ElementAt(track).Path));
                        b_play.IsEnabled = true;
                        b_play.Content = "►";
                        Scroll();
                    }
                }
            }
            else
            {
                if (wiederholen)
                {
                    track = 0;
                    if (playing)
                    {
                        Play();
                    }
                    else
                    {
                        if (Tracks.Count > track)
                        {
                            mp.Close();
                            mp.Open(new Uri(Tracks.ElementAt(track).Path));
                            b_play.IsEnabled = true;
                            b_play.Content = "►";
                            Scroll();
                        }
                    }
                }
                else
                {
                    playing = false;
                }
            }
        }

        private void Play()
        {
            if (Tracks.Count > track)
            {
                g_controls.IsEnabled = false;
                playing = true;
                mp.Close();
                mp.Open(new Uri(Tracks.ElementAt(track).Path));
                Scroll();
            }
        }

        private void Zurück()
        {
            if (mp.Position.TotalSeconds > 3)
            {
                mp.Stop();
                if (playing)
                {
                    mp.Play();
                }
            }
            else
            {
                Previous();
            }
        }

        private void Previous()
        {
            if (track - 1 >= 0)
            {
                track--;

                if (playing == false)
                {
                    mp.Close();
                    mp.Open(new Uri(Tracks.ElementAt(track).Path));
                    b_play.IsEnabled = true;
                    b_play.Content = "►";
                }
                else
                {
                    Play();
                }
            }
            else
            {
                track = Tracks.Count - 1;
                if (playing == false)
                {
                    mp.Close();
                    mp.Open(new Uri(Tracks.ElementAt(track).Path));
                    b_play.IsEnabled = true;
                    b_play.Content = "►";
                }
                else
                {
                    Play();
                }
            }
            if (lb_tracks.Items.Count == Tracks.Count && Tracks.Count > track)
            {
                lb_tracks.SelectedIndex = track;
                lb_tracks.ScrollIntoView(Tracks.ElementAt(track));
            }
        }

        private void Leeren()
        {
            if (File.Exists(playlist))
            {
                SavePlaylist();
            }
            b_play.IsEnabled = false;
            b_play.Content = "►";
            mp.Close();
            track = 0;
            Tracks.Clear();
            playlist = null;
            playing = false;
            l_info.Content = "Trackinfo     " + Environment.NewLine + Environment.NewLine;
            l_position.Content = "Spielzeit";
            info = "Trackinfo       " + Environment.NewLine + Environment.NewLine;
            s_position.Maximum = 0;
            s_position.Value = 0;
            i_cover.Source = new BitmapImage();
            i_covermini.Source = new BitmapImage();
            i_cover.Width = 0;
            i_cover.Height = 0;
            i_covermini.Width = 0;
            i_covermini.Height = 0;
        }

        private void StartScan(List<string> tracks)
        {
            new Thread(() => Scan(tracks)).Start();

            if (tracks.Count > 0)
            {
                laden = new Laden();
                laden.Owner = this;
                laden.ShowDialog();
            }
        }

        private void Scan(List<string> tracks)
        {
            timer.Start();
            threadabort = false;
            scanning = true;
            geladen = 1;
            zuladen = tracks.Count - Tracks.Count;
            List<Track> cache = new List<Track>();
            foreach (string path in tracks)
            {
                if (File.Exists(path))
                {
                    try
                    {
                        cache.Add(Track.ParseFromMP3(path));
                        geladen++;
                    }
                    catch (Exception ex)
                    {
                        InfoView("Scanfehler bei Track: " + path + "\n" + ex);
                    }
                }
                else
                {
                    InfoView("Der Track " + path + " konnte nicht geladen werden!");
                }
            }
            if (threadabort)
            {
                InfoView("Scan abgebrochen!");
            }
            scanning = false;
            Console.WriteLine("Scan durch!");
            w_player.Dispatcher.Invoke(() => Display(cache));
        }

        void fadeout_Tick(object sender, EventArgs e)
        {
            if (playing && mp.Volume > 0)
            {
                mp.Volume = mp.Volume - ((Double)((DispatcherTimer)sender).Tag);
            }
            else
            {
                ((DispatcherTimer)sender).Stop();
                this.Close();
            }
        }
        #endregion Mediaplayer        

        #region Display
        private void Display(List<Track> cache)
        {
            if (laden != null)
            {
                laden.Close();
            }

            foreach (Track t in cache)
            {
                Tracks.Add(t);
            }

            if (einstellungenladen)
            {
                einstellungenladen = false;
                if (mute)
                {
                    InfoView("Der Player ist gemuted. Die Wiedergabe hat so kein Audiosignal.");
                }
                if (playing)
                {
                    Play();
                }
                else
                {
                    if (track < Tracks.Count)
                    {
                        mp.Close();
                        mp.Open(new Uri(Tracks.ElementAt(track).Path));
                        b_play.Content = "►";
                        b_play.IsEnabled = true;
                    }
                }
                Start.Begin();
            }

            List<string> tracks = new List<string>();
            foreach (Track t in Tracks)
            {
                tracks.Add(t.Path);
            }
            if (tracks.SequenceEqual(ofd.FileNames) || file != null)
            {
                Play();
                file = null;
            }
        }        

        private void i_cover_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            i_cover.Height = i_cover.MaxHeight;
            i_cover.Width = i_cover.MaxWidth;
            l_info1.Margin = new Thickness(i_cover.ActualWidth + 20, 65, 35, 0);
        }

        private void l_info_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            i_covermini.Width = l_info.ActualHeight;
            i_covermini.Height = l_info.ActualHeight;
            l_info.Margin = new Thickness(l_info.ActualHeight + 5, 0, 0, 0);
            pb_position.Margin = new Thickness(0, l_info.ActualHeight + 10, 0, 0);
        }

        void timer_Tick(object sender, EventArgs e)
        {
            #region Tasten
            if (key && Keyboard.IsKeyDown(Key.MediaPlayPause) || key && Keyboard.IsKeyDown(Key.MediaNextTrack)
                || key && Keyboard.IsKeyDown(Key.MediaPreviousTrack) || key && Keyboard.IsKeyDown(Key.MediaStop)
                || key && Keyboard.IsKeyDown(Key.SelectMedia))
            {
                if (Keyboard.IsKeyDown(Key.MediaPlayPause))
                {
                    pressed = Key.MediaPlayPause;
                    PlayPause();
                }
                if (Keyboard.IsKeyDown(Key.MediaNextTrack))
                {
                    pressed = Key.MediaNextTrack;
                    Next();
                }
                if (Keyboard.IsKeyDown(Key.MediaPreviousTrack))
                {
                    pressed = Key.MediaPreviousTrack;
                    Zurück();
                }
                if (Keyboard.IsKeyDown(Key.MediaStop))
                {
                    pressed = Key.MediaStop;
                    playing = false;
                    mp.Stop();
                    b_play.Content = "►";
                }
                if (Keyboard.IsKeyDown(Key.SelectMedia))
                {
                    pressed = Key.SelectMedia;
                    playing = false;
                    ofd.ShowDialog();
                }

                key = false;
                Console.WriteLine("Key Detected: " + pressed);
            }
            else
            {
                if (pressed != Key.None && Keyboard.IsKeyUp(pressed))
                {
                    pressed = Key.None;
                    key = true;
                }

                l_zeit.Content = DateTime.Now.ToString();
                if (scanning)
                {
                    l_info.Content = "Wird gescannt..." + Environment.NewLine + geladen + " von " + zuladen + " abgeschlossen ";
                    l_info1.Content = l_info.Content;
                    s_position.Maximum = zuladen;
                    s_position.Value = geladen;
                    if (laden != null && laden.IsActive)
                    {
                        laden.Max = zuladen;
                        laden.Message = "ID3-Tags werden in die Datenbank geschrieben." + Environment.NewLine + geladen + " von " + zuladen + " Tracks abgeschlossen...";
                        laden.Status = geladen;
                    }
                }
                else
                {
                    if (mp.NaturalDuration.HasTimeSpan)
                    {
                        string inlist;
                        if (playlist != null)
                        {
                            inlist = " [" + playlist.Substring(playlistordner.Length, playlist.Length - playlistordner.Length).Replace(".plst", "") + "]";
                        }
                        else
                        {
                            inlist = null;
                        }
                        s_position.TickFrequency = Convert.ToDouble(mp.NaturalDuration.TimeSpan.TotalMilliseconds / 100);
                        s_position.Maximum = mp.NaturalDuration.TimeSpan.TotalMilliseconds;
                        s_position.Value = mp.Position.TotalMilliseconds;
                        l_info.Content = info + Environment.NewLine + mp.Position.ToString(@"hh\:mm\:ss\.ff") + "/" + mp.NaturalDuration.TimeSpan.ToString(@"hh\:mm\:ss\.ff") + Environment.NewLine + "Track: " + (track + 1) + "/" + Tracks.Count + inlist;
                        l_info1.Content = l_info.Content;
                        l_position.Content = mp.Position.ToString(@"hh\:mm\:ss\.ff") + "/" + mp.NaturalDuration.TimeSpan.ToString(@"hh\:mm\:ss\.ff");
                    }
                    else
                    {
                        if (mp.HasAudio && Tracks.Count > 0)
                        {
                            s_position.TickFrequency = 1;
                            s_position.Maximum = 100;
                            s_position.Value = mp.Position.TotalMilliseconds;
                            l_info.Content = info + Environment.NewLine + mp.Position.ToString(@"hh\:mm\:ss\.ff") + " [Stream]" + Environment.NewLine + "Tracks: " + (track + 1) + "/" + Tracks.Count;
                            l_info1.Content = l_info.Content;
                            l_position.Content = mp.Position.ToString(@"hh\:mm\:ss\.ff");
                        }
                        else
                        {
                            s_position.Maximum = 0;
                            s_position.Value = 0;
                            l_info.Content = "Trackinfo     " + Environment.NewLine + Environment.NewLine;
                            l_info1.Content = l_info.Content;
                        }
                    }
            }
            #endregion Tasten
            
            }
        }        

        void delay_Tick(object sender, EventArgs e)
        {
            delay.Stop();
            minimize();
        }

        private void minimize()
        {
            timer.Stop();
            double t = .8;         //Animationsdauer
            DoubleAnimation widthanimation = new DoubleAnimation(100, TimeSpan.FromSeconds(t));
            Storyboard.SetTarget(widthanimation, g_controls);
            Storyboard.SetTargetProperty(widthanimation, new PropertyPath(Grid.WidthProperty));

            DoubleAnimation heightanimation = new DoubleAnimation(0, TimeSpan.FromSeconds(t));
            Storyboard.SetTarget(heightanimation, g_controls);
            Storyboard.SetTargetProperty(heightanimation, new PropertyPath(Grid.HeightProperty));

            DoubleAnimation opacityanimation = new DoubleAnimation(0, TimeSpan.FromSeconds(t / 2));
            Storyboard.SetTarget(opacityanimation, g_controls);
            Storyboard.SetTargetProperty(opacityanimation, new PropertyPath(Grid.OpacityProperty));

            ObjectAnimationUsingKeyFrames visibilityanimation = new ObjectAnimationUsingKeyFrames();
            Storyboard.SetTarget(visibilityanimation, g_controls);
            Storyboard.SetTargetProperty(visibilityanimation, new PropertyPath(Grid.VisibilityProperty));
            DiscreteObjectKeyFrame kf = new DiscreteObjectKeyFrame(Visibility.Collapsed, TimeSpan.FromSeconds(t));
            visibilityanimation.KeyFrames.Add(kf);

            ObjectAnimationUsingKeyFrames lbvisibilityanimation = new ObjectAnimationUsingKeyFrames();
            Storyboard.SetTarget(lbvisibilityanimation, lb_tracks);
            Storyboard.SetTargetProperty(lbvisibilityanimation, new PropertyPath(ListBox.VisibilityProperty));
            DiscreteObjectKeyFrame kf2 = new DiscreteObjectKeyFrame(Visibility.Collapsed, TimeSpan.FromSeconds(0.1));
            lbvisibilityanimation.KeyFrames.Add(kf2);

            ObjectAnimationUsingKeyFrames minivisibilityanimation = new ObjectAnimationUsingKeyFrames();
            visibilityanimation.Duration = TimeSpan.FromSeconds(t);
            Storyboard.SetTarget(minivisibilityanimation, g_info);
            Storyboard.SetTargetProperty(minivisibilityanimation, new PropertyPath(Grid.VisibilityProperty));
            DiscreteObjectKeyFrame kf1 = new DiscreteObjectKeyFrame(Visibility.Visible, new TimeSpan());
            minivisibilityanimation.KeyFrames.Add(kf1);

            DoubleAnimation miniopacityanimation = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(t));
            Storyboard.SetTarget(miniopacityanimation, g_info);
            Storyboard.SetTargetProperty(miniopacityanimation, new PropertyPath(Grid.OpacityProperty));

            DoubleAnimation lbopacityanimation = new DoubleAnimation(0, TimeSpan.FromSeconds(0.1));
            Storyboard.SetTarget(lbopacityanimation, lb_tracks);
            Storyboard.SetTargetProperty(lbopacityanimation, new PropertyPath(ListBox.OpacityProperty));

            Storyboard sb = new Storyboard();
            sb.Completed += sb_Completed;
            sb.Children.Add(widthanimation);
            sb.Children.Add(heightanimation);
            sb.Children.Add(opacityanimation);
            sb.Children.Add(visibilityanimation);
            sb.Children.Add(minivisibilityanimation);
            sb.Children.Add(miniopacityanimation);
            sb.Children.Add(lbopacityanimation);
            sb.Children.Add(lbvisibilityanimation);
            sb.Begin();
        }

        void sb_Completed(object sender, EventArgs e)
        {
            timer.Start();
        }

        private void maximize()
        {
            timer.Stop();
            double t = .8;         //Animationsdauer
            DoubleAnimation widthanimation = new DoubleAnimation(600, TimeSpan.FromSeconds(t));
            Storyboard.SetTarget(widthanimation, g_controls);
            Storyboard.SetTargetProperty(widthanimation, new PropertyPath(Grid.WidthProperty));

            DoubleAnimation heightanimation = new DoubleAnimation(350, TimeSpan.FromSeconds(t));
            Storyboard.SetTarget(heightanimation, g_controls);
            Storyboard.SetTargetProperty(heightanimation, new PropertyPath(Grid.HeightProperty));

            DoubleAnimation opacityanimation = new DoubleAnimation(1, TimeSpan.FromSeconds(t/3));
            opacityanimation.BeginTime = TimeSpan.FromSeconds(t/3);
            Storyboard.SetTarget(opacityanimation, g_controls);
            Storyboard.SetTargetProperty(opacityanimation, new PropertyPath(Grid.OpacityProperty));

            ObjectAnimationUsingKeyFrames visibilityanimation = new ObjectAnimationUsingKeyFrames();
            Storyboard.SetTarget(visibilityanimation, g_controls);
            Storyboard.SetTargetProperty(visibilityanimation, new PropertyPath(Grid.VisibilityProperty));
            DiscreteObjectKeyFrame kf = new DiscreteObjectKeyFrame(Visibility.Visible, TimeSpan.FromSeconds(0));
            visibilityanimation.KeyFrames.Add(kf);

            ObjectAnimationUsingKeyFrames lbvisibilityanimation = new ObjectAnimationUsingKeyFrames();
            Storyboard.SetTarget(lbvisibilityanimation, lb_tracks);
            Storyboard.SetTargetProperty(lbvisibilityanimation, new PropertyPath(ListBox.VisibilityProperty));
            DiscreteObjectKeyFrame kf2 = new DiscreteObjectKeyFrame(Visibility.Visible, TimeSpan.FromSeconds(t - 0.1));
            lbvisibilityanimation.KeyFrames.Add(kf2);

            ObjectAnimationUsingKeyFrames minivisibilityanimation = new ObjectAnimationUsingKeyFrames();
            minivisibilityanimation.Duration = TimeSpan.FromSeconds(0.1);            minivisibilityanimation.BeginTime = TimeSpan.FromSeconds(t - 0.1);
            Storyboard.SetTarget(minivisibilityanimation, g_info);
            Storyboard.SetTargetProperty(minivisibilityanimation, new PropertyPath(Grid.VisibilityProperty));
            DiscreteObjectKeyFrame kf1 = new DiscreteObjectKeyFrame(Visibility.Collapsed, TimeSpan.FromSeconds(t));
            minivisibilityanimation.KeyFrames.Add(kf1);

            DoubleAnimation miniopacityanimation = new DoubleAnimation(0, TimeSpan.FromSeconds(t));
            Storyboard.SetTarget(miniopacityanimation, g_info);
            Storyboard.SetTargetProperty(miniopacityanimation, new PropertyPath(Grid.OpacityProperty));

            DoubleAnimation lbopacityanimation = new DoubleAnimation(1, TimeSpan.FromSeconds(0.1));
            lbopacityanimation.BeginTime = TimeSpan.FromSeconds(t - 0.1);
            Storyboard.SetTarget(lbopacityanimation, lb_tracks);
            Storyboard.SetTargetProperty(lbopacityanimation, new PropertyPath(ListBox.OpacityProperty));

            Storyboard sb = new Storyboard();
            sb.Completed += sb_Completed;
            sb.Children.Add(widthanimation);
            sb.Children.Add(heightanimation);
            sb.Children.Add(opacityanimation);
            sb.Children.Add(visibilityanimation);
            sb.Children.Add(minivisibilityanimation);
            sb.Children.Add(miniopacityanimation);
            sb.Children.Add(lbopacityanimation);
            sb.Children.Add(lbvisibilityanimation);
            sb.Begin();
        }

        private void einklappen()
        {
            double t = .4;         //Animationsdauer
            DoubleAnimation animation = new DoubleAnimation(50, TimeSpan.FromSeconds(t));
            animation.Completed +=animation_Completed;
            lb_tracks.BeginAnimation(ListBox.HeightProperty, animation);
        }

        private void ausklappen()
        {
            double t = .4;         //Animationsdauer
            DoubleAnimation animation = new DoubleAnimation(290, TimeSpan.FromSeconds(t));
            animation.Completed += animation_Completed;
            lb_tracks.BeginAnimation(ListBox.HeightProperty, animation);
            Scroll();
        }

        void animation_Completed(object sender, EventArgs e)
        {
            if (lb_tracks.ActualHeight > 100)
                lb_tracks.SetValue(ScrollViewer.VerticalScrollBarVisibilityProperty, ScrollBarVisibility.Auto);
            else
                lb_tracks.SetValue(ScrollViewer.VerticalScrollBarVisibilityProperty, ScrollBarVisibility.Disabled);
            Scroll();
        }
        #endregion Display

        #region Buttons
        private void b_zurück_Click(object sender, RoutedEventArgs e)
        {
            Zurück();
        }

        private void b_vor_Click(object sender, RoutedEventArgs e)
        {
            Next();
        }               

        private void b_play_Click(object sender, RoutedEventArgs e)
        {
            PlayPause();
        }       

        private void b_laden_Click(object sender, RoutedEventArgs e)
        {
            ofd.ShowDialog();
        }

        private void b_schließen_Click(object sender, RoutedEventArgs e)
        {
            w_player.Visibility = Visibility.Collapsed;
            DispatcherTimer fadeout = new DispatcherTimer();
            fadeout.Interval = new TimeSpan(0, 0, 0, 0, 100);
            fadeout.Tick += fadeout_Tick;
            fadeout.Tag = mp.Volume / 10;
            fadeout.Start();
        }

        private void b_leeren_Click(object sender, RoutedEventArgs e)
        {
            if (playing == false
                || DialogView("Es wird noch Musik gespielt.\nMöchten sie die Wiedergabe wirklich leeren?"))
            {
                Leeren();
            }
        }        
        #endregion Buttons

        #region Slider
        private void s_lautstärke_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mp.Volume = s_lautstärke.Value*0.01;
        }

        private void s_balance_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mp.Balance = s_balance.Value;
        }

        private void s_balance_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            s_balance.Value = 0;
        }

        private void s_lautstärke_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            s_lautstärke.Value = 80;
        }        

        private void s_position_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            pb_position.Maximum = s_position.Maximum;
            pb_position.Value = s_position.Value;
            if (s_position.IsMouseOver && mp.NaturalDuration.HasTimeSpan)
            {
                mp.Position = new TimeSpan(0, 0, 0, 0, Convert.ToInt32(s_position.Value));
            }
        }                

        private void s_speed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
                speed = s_speed.Value;
                mp.SpeedRatio = speed;
                l_speed.Content = "Geschwindigkeit: " + s_speed.Value.ToString("0.00") + "x";                    
        }

        private void s_speed_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            s_speed.Value = 1;
        }

        private void s_position_ToolTipOpening(object sender, ToolTipEventArgs e)
        {
            s_position.ToolTip = mp.Position.ToString(@"hh\:mm\:ss\:ff");
        }
        #endregion Slider

        #region Checkboxes
        private void cb_mute_Checked(object sender, RoutedEventArgs e)
        {
            mute = true;
            mp.IsMuted = true;
        }

        private void cb_mute_Unchecked(object sender, RoutedEventArgs e)
        {
            mute = false;
            mp.IsMuted = false;
        }

        private void cb_schwarz_Checked(object sender, RoutedEventArgs e)
        {
            schwarz = true;
            SolidColorBrush brush = new SolidColorBrush(Colors.Black);
            l_info.Foreground = brush;
            l_info1.Foreground = brush;
            l_position.Foreground = brush;
            l_speed.Foreground = brush;
            l_zeit.Foreground = brush;
            cb_mute.Foreground = brush;
            cb_positonmerken.Foreground = brush;
            cb_schwarz.Foreground = brush;
            cb_wiederholen.Foreground = brush;
            cb_zufall.Foreground = brush;
            s_balance.Foreground = brush;
            s_lautstärke.Foreground = brush;
            s_position.Foreground = brush;
            s_speed.Foreground = brush;

            Color c1 = new Color();
            Color c2 = new Color();
            c1 = Color.FromArgb(255, Colors.White.R, Colors.White.G, Colors.White.B);
            c2 = Color.FromArgb(200, Colors.Black.R, Colors.Black.G, Colors.Black.B);
            LinearGradientBrush brush2 = new LinearGradientBrush(c1, c2, 90);
            pb_position.Foreground = brush2;
        }

        private void cb_schwarz_Unchecked(object sender, RoutedEventArgs e)
        {
            schwarz = false;
            SolidColorBrush brush = new SolidColorBrush(Colors.White);
            l_info.Foreground = brush;
            l_info1.Foreground = brush;
            l_position.Foreground = brush;
            l_speed.Foreground = brush;
            l_zeit.Foreground = brush;
            cb_mute.Foreground = brush;
            cb_positonmerken.Foreground = brush;
            cb_schwarz.Foreground = brush;
            cb_wiederholen.Foreground = brush;
            cb_zufall.Foreground = brush;
            s_balance.Foreground = brush;
            s_lautstärke.Foreground = brush;
            s_position.Foreground = brush;
            s_speed.Foreground = brush;

            Color c1 = new Color();
            c1 = Color.FromArgb(0, Colors.White.R, Colors.White.G, Colors.White.B);
            LinearGradientBrush brush2 = new LinearGradientBrush(Colors.White, c1, 90);
            pb_position.Foreground = brush2;
        }

        private void cb_zufall_Checked(object sender, RoutedEventArgs e)
        {
            zufall = true;
        }

        private void cb_zufall_Unchecked(object sender, RoutedEventArgs e)
        {
            zufall = false;
        }

        private void cb_positonmerken_Checked(object sender, RoutedEventArgs e)
        {
            positionmerken = true;
        }

        private void cb_positonmerken_Unchecked(object sender, RoutedEventArgs e)
        {
            positionmerken = false;
        }

        private void cb_wiederholen_Unchecked(object sender, RoutedEventArgs e)
        {
            wiederholen = false;
        }

        private void cb_wiederholen_Checked(object sender, RoutedEventArgs e)
        {
            wiederholen = true;
        }
        #endregion Checkboxes

        #region ListBox
        #region ContextMenu
        private void lb_tracks_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (lb_tracks.SelectedItem == null)
            {
                mi_entfernen.IsEnabled = false;
                mi_oben.IsEnabled = false;
                mi_unten.IsEnabled = false;
                mi_verschieben.IsEnabled = false;
                mi_vonvorn.IsEnabled = false;
                mi_umbennenen.IsEnabled = false;
                mi_hinzufügen.IsEnabled = false;
            }
            else
            {
                mi_entfernen.IsEnabled = true;
                mi_verschieben.IsEnabled = true;
                mi_vonvorn.IsEnabled = true;
                mi_umbennenen.IsEnabled = true;
                mi_hinzufügen.IsEnabled = true;
                if (lb_tracks.SelectedIndex > 0)
                {
                    mi_oben.IsEnabled = true;
                }
                else
                {
                    mi_oben.IsEnabled = false;
                }

                if (lb_tracks.Items.Count > lb_tracks.SelectedIndex + 1)
                {
                    mi_unten.IsEnabled = true;
                }
                else
                {
                    mi_unten.IsEnabled = false;
                }
            }

            if (lb_tracks.ItemsSource == Playlists)
            {
                mi_playlists.Header = "Aktuelle Wiedergabe";
                mi_oben.Visibility = Visibility.Collapsed;
                mi_unten.Visibility = Visibility.Collapsed;
                mi_verschieben.Visibility = Visibility.Collapsed;
                mi_vonvorn.Visibility = Visibility.Collapsed;
                mi_neu.Visibility = Visibility.Visible;
                mi_umbennenen.Visibility = Visibility.Visible;
                mi_hinzufügen.Visibility = Visibility.Visible;
                mi_laden.Visibility = Visibility.Visible;
            }
            else
            {
                mi_playlists.Header = "Playlists";
                mi_oben.Visibility = Visibility.Visible;
                mi_unten.Visibility = Visibility.Visible;
                mi_verschieben.Visibility = Visibility.Visible;
                mi_vonvorn.Visibility = Visibility.Visible;
                mi_neu.Visibility = Visibility.Collapsed;
                mi_umbennenen.Visibility = Visibility.Collapsed;
                mi_hinzufügen.Visibility = Visibility.Collapsed;
                mi_laden.Visibility = Visibility.Collapsed;
            }
        }

        private void mi_oben_Click(object sender, RoutedEventArgs e)
        {
            int index = lb_tracks.SelectedIndex;
            if (lb_tracks.SelectedIndex == track)
            {
                track--;
            }
            else
            {
                if (lb_tracks.SelectedIndex == track + 1)
                {
                    track++;
                }
            }
            Tracks.Move(index, index - 1);
        }

        private void mi_tauschen_Click(object sender, RoutedEventArgs e)
        {
            dragging = true;
            moveindex = lb_tracks.SelectedIndex;
            Console.WriteLine("To Move: " + moveindex);
        }

        private void mi_playlists_Click(object sender, RoutedEventArgs e)
        {
            if (lb_tracks.ItemsSource == Tracks)
            {
                Playlists.Clear();
                PlaylistsAktualisieren();
                lb_tracks.ItemTemplate = (DataTemplate)FindResource("PlaylistTemplate");
                lb_tracks.ItemsSource = Playlists;
            }
            else
            {
                lb_tracks.ItemTemplate = (DataTemplate)FindResource("BenutzerTemplate");
                lb_tracks.ItemsSource = Tracks;
            }
        }

        private void mi_neu_Click(object sender, RoutedEventArgs e)
        {
            string res = PlaylistName();
            if (res != null)
            {
                string newplst = playlistordner + res + ".plst";
                if (!File.Exists(newplst))
                {
                    if (DialogView("Möchten sie die aktuelle Wiedergabe ind die neue Playlist übertragen?"))
                    {
                        playlist = newplst;
                        SavePlaylist();
                    }
                    else
                    {
                        XDocument xd = new XDocument();
                        xd.Add(new XElement("Playlist"));
                        xd.Save(newplst);
                    }
                }
                else
                {
                    InfoView("Die Playlist " + res + " existiert bereits!");
                }
            }
            PlaylistsAktualisieren();
        }

        private void mi_hinzufügen_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string plst = playlistordner + lb_tracks.SelectedItem.ToString() + ".plst";
                if (File.Exists(plst))
                {
                    List<string> temp = new List<string>();
                    XDocument tempreader = XDocument.Load(plst);
                    if (tempreader.Element("Playlist") != null && tempreader.Element("Playlist").Element("Track") != null)
                    {
                        for (int i = 0; i < tempreader.Element("Playlist").Elements("Track").Count(); i++)
                        {
                            if (tempreader.Element("Playlist").Elements("Track").ElementAt(i).Attribute("path") != null)
                                temp.Add(tempreader.Element("Playlist").Elements("Track").ElementAt(i).Attribute("path").Value.ToString());
                        }
                    }

                    List<string> tracks = new List<string>();
                    foreach (Track t in Tracks)
                    {
                        tracks.Add(t.Path);
                    }
                    temp.AddRange(tracks);

                    XDocument Datenbank = new XDocument();
                    Datenbank.Add(new XElement("Playlist"));
                    Datenbank.Element("Playlist").Add
                    (
                        new XAttribute("name", plst.Substring(playlistordner.Length, plst.Length - playlistordner.Length))
                    );

                    foreach (string path in temp)
                    {
                        Datenbank.Element("Playlist").Add
                        (   
                            new XElement
                            (
                                "Track",
                                new XAttribute("path", path),
                                new XAttribute("name", "")
                            )
                        );
                    }
                    Datenbank.Save(plst);
                }
                else
                {
                    InfoView("Dateifehler! Die Datei ist nichtmehr vorhanden.");
                }
            }
            catch
            {
               InfoView("Fehler!");
            }
            PlaylistsAktualisieren();
        }

        private void mi_umbenennen_Click(object sender, RoutedEventArgs e)
        {
            string oldname = ((Playlist)lb_tracks.SelectedItem).Path;
            string res = PlaylistName();
            string newname = (playlistordner + res + ".plst");                        
            if (res != null)
            {
                if (!File.Exists(newname) && newname.ToLower() != oldname.ToLower())
                {
                    if (File.Exists(oldname))
                    {
                        string[] sa = File.ReadAllLines(oldname);
                        File.WriteAllLines(newname, sa);
                        File.Delete(oldname);
                        if (playlist == oldname)
                        {
                            playlist = newname;
                        }
                    }
                }
                else
                {
                    InfoView("Die Playlist " + res + " existiert bereits!");
                }
            }
            PlaylistsAktualisieren();
        }

        private void mi_vonvorn_Click(object sender, RoutedEventArgs e)
        {
            if (Tracks.Count > 0)
            {
                track = 0;
                Play();
            }
        }

        private void mi_entfernen_Click(object sender, RoutedEventArgs e)
        {
            if (lb_tracks.ItemsSource == Tracks)
            {
                if (lb_tracks.SelectedIndex != track
                    || DialogView("Der zu löschende Track wird gerade gespielt! Möchten sie ihn dennoch löschen?"))
                {
                    int index = lb_tracks.SelectedIndex;
                    if (index == track)
                    {
                        mp.Close();
                        l_info.Content = "Trackinfo";
                        b_play.Content = "►";
                        s_position.Value = 0;
                        l_position.Content = "Spielzeit";
                        info = "";
                        Tracks.RemoveAt(index);

                        if (Tracks.Count - 1 > track)
                        {
                            if (zufall)
                            {
                                Random r = new Random();
                                int lasttrack = track;
                                while (track == lasttrack)
                                {
                                    track = r.Next(Tracks.Count - 1);
                                }
                            }
                            if (playing)
                            {
                                Play();
                            }
                            else
                            {
                                if (Tracks.Count > track)
                                {
                                    mp.Close();
                                    mp.Open(new Uri(Tracks.ElementAt(track).Path));
                                    b_play.IsEnabled = true;
                                    b_play.Content = "►";
                                    Scroll();
                                }
                            }
                        }
                        else
                        {
                            if (wiederholen)
                            {
                                track = 0;
                                if (playing)
                                {
                                    Play();
                                }
                                else
                                {
                                    if (Tracks.Count > track)
                                    {
                                        mp.Close();
                                        mp.Open(new Uri(Tracks.ElementAt(track).Path)); 
                                        b_play.IsEnabled = true;
                                        b_play.Content = "►";
                                        Scroll();
                                    }
                                }
                            }
                            else
                            {
                                playing = false;
                            }
                        }

                    }
                    else
                    {
                        Tracks.RemoveAt(index);
                    }
                }
            }
            else
            {
                if (DialogView("Möchten sie die Playlist " + ((Playlist)lb_tracks.SelectedItem).Name + " wirklich löschen?"))
                {
                    try
                    {
                        string oldlist = ((Playlist)lb_tracks.SelectedItem).Path;
                        File.Delete(oldlist);
                        Playlists.Remove((Playlist)lb_tracks.SelectedItem);
                        if (playlist == oldlist)
                        {
                            playlist = null;
                        }
                    }
                    catch (Exception ex) { InfoView("Fehler: " + ex); }
                }
            }
            PlaylistsAktualisieren();
        }
           
        private void mi_unten_Click(object sender, RoutedEventArgs e)
        {
            int index = lb_tracks.SelectedIndex;
            if (lb_tracks.SelectedIndex == track)
            {
                track++;
            }
            else
            {
                if (lb_tracks.SelectedIndex == track - 1)
                {
                    track--;
                }
            }
            Tracks.Move(index, index + 1);
        }

        private void mi_laden_Click(object sender, RoutedEventArgs e)
        {
            oplst.ShowDialog();
        }

        void oplst_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (File.Exists(oplst.FileName))
            {
                string plstfile = oplst.FileName;
                FileInfo fi = new FileInfo(plstfile);
                if (File.Exists(playlistordner + fi.Name) == false)
                {
                    if (fi.Extension == ".plst")
                    {
                        File.Copy(plstfile, playlistordner + fi.Name);

                        Leeren();
                        playlist = playlistordner + fi.Name;
                        track = 0;
                        playing = true;
                        einstellungenladen = true;
                        LoadPlaylist(Playlist.Create(playlistordner + fi.Name));
                        lb_tracks.ItemTemplate = (DataTemplate)FindResource("BenutzerTemplate");
                        lb_tracks.ItemsSource = Tracks;
                    }
                    if (fi.Extension == ".zpl" || fi.Extension == ".wpl")
                    {
                        Leeren();
                        XDocument xd = XDocument.Load(plstfile);
                        if (xd.Element("smil") != null && xd.Element("smil").Element("body") != null && xd.Element("smil").Element("body").Element("seq") != null
                            && xd.Element("smil").Element("body").Element("seq").Element("media") != null)
                        {
                            List<string> tracks = new List<string>();
                            foreach (XElement xe in xd.Element("smil").Element("body").Element("seq").Elements("media"))
                            {
                                if (xe.Attribute("src") != null)
                                {
                                    string s = xe.Attribute("src").Value.Replace(@"..\",              Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) + @"\");
                                    tracks.Add(s);
                                }
                            }
                            StartScan(tracks);
                        }
                        playlist = null;
                        track = 0;
                        playing = true;
                        einstellungenladen = true;
                        lb_tracks.ItemTemplate = (DataTemplate)FindResource("BenutzerTemplate");
                        lb_tracks.ItemsSource = Tracks;
                    }
                }
                else
                {
                    InfoView("Die Playlist '" + fi.Name + "' ist bereits vorhanden!");                    
                    Leeren();
                    playlist = plstfile;
                    track = 0;
                    playing = true;
                    einstellungenladen = true;
                    LoadPlaylist(Playlist.Create(plstfile));
                    lb_tracks.ItemTemplate = (DataTemplate)FindResource("BenutzerTemplate");
                    lb_tracks.ItemsSource = Tracks;
                }
            }
        }
        #endregion ContextMenu

        private void lb_tracks_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lb_tracks.ItemsSource == Tracks)
            {
                if (lb_tracks.SelectedItem != null)
                {
                    track = lb_tracks.SelectedIndex;
                    Play();
                    lb_tracks.SetValue(ScrollViewer.VerticalScrollBarVisibilityProperty, ScrollBarVisibility.Disabled);
                    ausklappencomplete = false;
                    einklappen();
                }
            }
            else
            {
                if (lb_tracks.SelectedItem != null)
                {
                    if (File.Exists(((Playlist)lb_tracks.SelectedItem).Path))
                    {                                               
                        Leeren();
                        playlist = ((Playlist)lb_tracks.SelectedItem).Path;
                        track = 0;
                        playing = true;
                        einstellungenladen = true;
                        LoadPlaylist(Playlist.Create(((Playlist)lb_tracks.SelectedItem).Path));
                        lb_tracks.ItemTemplate = (DataTemplate)FindResource("BenutzerTemplate");
                        lb_tracks.ItemsSource = Tracks;
                        delay.Stop();
                        ausklappen();                        
                    }
                }
            }
        }

        private void lb_tracks_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (lb_tracks.ItemsSource == Tracks)
            {
                Console.WriteLine("MouseUp!");
                if (dragging == true && lb_tracks.SelectedIndex >= 0 && moveindex >= 0 && lb_tracks.SelectedIndex != moveindex)
                {
                    dragging = false;
                    int movetoindex = lb_tracks.SelectedIndex;
                    if (moveindex == track)
                    {
                        track = movetoindex;
                    }
                    else
                    {
                        if (track == movetoindex)
                        {
                            track = moveindex;
                        }
                        else
                        {
                            if (moveindex < track && movetoindex > track)
                            {
                                track--;
                            }
                            else
                            {
                                if (moveindex > track && movetoindex < track)
                                {
                                    track++;
                                }
                            }
                        }
                    }
                    Tracks.Move(moveindex, movetoindex);
                    Console.WriteLine("Move: " + moveindex + " to " + movetoindex);
                    lb_tracks.SelectedIndex = -1;
                    moveindex = -1;
                }
            }
            else
            {

            }
        }        
        
        private void lb_tracks_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (animationcomplete && lb_tracks.Height < 100)
            {
                ausklappen();
            }
        }

        private void lb_tracks_MouseLeave(object sender, MouseEventArgs e)
        {
            if (lb_tracks.ActualHeight > 50 && lb_tracks.ContextMenu.IsVisible == false && lb_tracks.IsMouseOver == false)
            {
                lb_tracks.SetValue(ScrollViewer.VerticalScrollBarVisibilityProperty, ScrollBarVisibility.Disabled);
                einklappen();
                dragging = false;
            }
        }  

        private void PlaylistsAktualisieren()
        {
            Playlists.Clear();
            if (Directory.Exists(playlistordner))
            {
                string[] sa = Directory.GetFiles(playlistordner);
                foreach (string s in sa)
                {
                    Playlists.Add(Playlist.Create(s));
                }
            }
        }
        #endregion ListBox

        #region Fenster
        private void w_player_MouseEnter(object sender, MouseEventArgs e)
        {           
            delay.Stop();
            if (g_controls.Visibility != Visibility.Visible)
                maximize();
        }

        private void w_player_MouseLeave(object sender, MouseEventArgs e)
        {
            if (w_player.IsMouseOver == false && g_controls.IsMouseOver == false && lb_tracks.ContextMenu.IsVisible == false)
            {
                delay.Start();
            }
        }

        private void w_player_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
        
        #region SpeicherDaten
        private void w_player_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Save();
        }

        private void Save()
        {
            try
            {
                if (playlist == null && Tracks.Count > 0
                && DialogView("Es ist keine Wiedergabeliste aktiv. Möchten sie eine Wiedergabeliste erstellen, damit die aktuelle Wiedergabe nicht verloren geht?"))
                {
                    string res = PlaylistName();
                    if (res != null)
                    {
                        string newplst = playlistordner + res + ".plst";
                        if (File.Exists(newplst) == false)
                        {
                            playlist = newplst;
                            SavePlaylist();
                            Playlists.Add(Playlist.Create(newplst));
                            playlist = newplst;
                        }
                        else
                        {
                            InfoView("Die Playlist " + res + " existiert bereits!");
                            Save();
                        }
                    }
                }

                if (Directory.Exists(path) == false)
                {
                    Directory.CreateDirectory(path);
                }

                if (File.Exists(playlist))
                {
                    SavePlaylist();
                }

                XDocument Datenbank = new XDocument();
                Datenbank.Add(new XElement("Configuration"));
                Datenbank.Element("Configuration").Add
                (
                    new XAttribute("target", "Player"),
                    new XElement("Player"),
                    new XElement("Design")
                );

                Datenbank.Element("Configuration").Element("Player").Add
                (
                    new XAttribute("track", track.ToString()),
                    new XAttribute("playing", playing.ToString()),
                    new XAttribute("repeat", wiederholen.ToString()),
                    new XAttribute("rememberposition", positionmerken.ToString()),
                    new XAttribute("position", mp.Position.TotalSeconds.ToString("0")),
                    new XAttribute("volume", s_lautstärke.Value.ToString()),
                    new XAttribute("random", zufall.ToString()),
                    new XAttribute("muting", mute.ToString()),                
                    new XAttribute("speedratio", s_speed.Value.ToString())
                );
                if (playlist != null)
                {
                    Datenbank.Element("Configuration").Element("Player").Add
                    (
                        new XAttribute("playlist", playlist.ToString())
                    );
                }

                Datenbank.Element("Configuration").Element("Design").Add
                (
                    new XAttribute("blackfont", schwarz.ToString())
                );
                Datenbank.Save(savefile);
            }
            catch (Exception ex)
            {
                InfoView("Speicherfehler: " + Environment.NewLine + ex);
            }
        }

        private void SavePlaylist()
        {
            try
            {
                if (playlist == null)
                {
                    InfoView("Keine speicherbare Playlist!");
                    return;
                }

                XDocument Datenbank = new XDocument();
                Datenbank.Add(new XElement("Playlist"));
                Datenbank.Element("Playlist").Add
                (
                    new XAttribute("name", playlist.Substring(playlistordner.Length, playlist.Length - playlistordner.Length)),
                    new XAttribute("path", playlist)
                );

                foreach (Track track in Tracks)
                {
                    if (track.Path != null)
                    {
                        XElement xe = new XElement
                            ("Track",
                            new XAttribute("path", track.Path),
                            new XAttribute("index", Tracks.IndexOf(track).ToString())
                                );

                        if (track.Performer != null)
                            xe.Add
                            (
                                new XAttribute("performer", track.Performer)
                            );
                        if (track.Title != null)
                            xe.Add
                            (
                                new XAttribute("title", track.Title)
                            );
                        if (track.Album != null)
                            xe.Add
                            (
                                new XAttribute("album", track.Album)
                            );
                        Datenbank.Element("Playlist").Add(xe);
                    }
                }
                Datenbank.Save(playlist);
            }
            catch
            {
                InfoView("Fehler beim sichern der Playlist!");
            }
        }

        private void LoadPlaylist(Playlist plst)
         {
            if (plst != null && File.Exists(plst.Path))
            {
                try
                {
                    Tracks.Clear();
                    playlist = plst.Path;
                    XDocument xd = XDocument.Load(plst.Path);
                    if (xd.Element("Playlist") != null && xd.Element("Playlist").Element("Track") != null)
                    {
                        List<string> tracks = new List<string>();
                        foreach (XElement xe in xd.Element("Playlist").Elements("Track"))
                        {
                            if (xe.Attribute("path") != null)
                                tracks.Add(xe.Attribute("path").Value );
                        }
                        StartScan(tracks);
                    }
                }
                catch
                {
                    InfoView("Die Playlist kann nicht geladen werden!" + Environment.NewLine +
                            "Ist sie beschädigt oder mit einer alten Version der Players gespeichert?");
                }
            }
        }

        private void w_player_Loaded(object sender, RoutedEventArgs e)
        {
            MemoryStream ms = new MemoryStream();
            Player.Properties.Resources.icon.Save(ms);
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.StreamSource = ms;
            bi.EndInit();
            w_player.Icon = bi;
            w_player.Left = System.Windows.SystemParameters.PrimaryScreenWidth - (g_controls.Width + 100);

            lb_tracks.ItemTemplate = (DataTemplate)FindResource("BenutzerTemplate");
            lb_tracks.ItemsSource = Tracks;
            LinearGradientBrush lgb = pb_position.Foreground as LinearGradientBrush;
            lgb = new LinearGradientBrush(lgb.GradientStops, 90);
            pb_position.Foreground = lgb;

            LinearGradientBrush lgb1 = b_hintergrund.Background as LinearGradientBrush;
            lgb = new LinearGradientBrush(lgb1.GradientStops, 45);
            b_hintergrund.Background = lgb1;
            try
            {
                if (File.Exists(savefile))
                {
                    einstellungenladen = true;
                    string s;
                    XDocument xd = XDocument.Parse(File.ReadAllText(savefile));
                    if (xd.Root.Element("Player").Attribute("track").Value != null)
                    {
                        s = xd.Root.Element("Player").Attribute("track").Value;
                        track = Convert.ToInt32(s);
                    }

                    if (xd.Root.Element("Player").Attribute("playing") != null)
                    {
                        s = xd.Root.Element("Player").Attribute("playing").Value;
                        playing = MakeBool(s);
                    }

                    if (xd.Root.Element("Player").Attribute("speedratio") != null)
                    {
                        s = xd.Root.Element("Player").Attribute("speedratio").Value;
                        s_speed.Value = Convert.ToDouble(s);
                    }

                    if (xd.Root.Element("Player").Attribute("volume") != null)
                    {
                        s = xd.Root.Element("Player").Attribute("volume").Value;
                        s_lautstärke.Value = Convert.ToDouble(s);
                    }

                    if (xd.Root.Element("Player").Attribute("random") != null)
                    {
                        s = xd.Root.Element("Player").Attribute("random").Value;
                        cb_zufall.IsChecked = MakeBool(s);
                    }

                    if (xd.Root.Element("Player").Attribute("muting") != null)
                    {
                        s = xd.Root.Element("Player").Attribute("muting").Value;
                        cb_mute.IsChecked = MakeBool(s);
                    }

                    if (xd.Root.Element("Player").Attribute("repeat") != null)
                    {
                        s = xd.Root.Element("Player").Attribute("repeat").Value;
                        cb_wiederholen.IsChecked = MakeBool(s);
                    }

                    if (xd.Root.Element("Player").Attribute("rememberposition") != null)
                    {
                        s = xd.Root.Element("Player").Attribute("rememberposition").Value;
                        cb_positonmerken.IsChecked = MakeBool(s);
                    }
                    
                    if (cb_positonmerken.IsChecked == true && xd.Root.Element("Player").Attribute("position") != null)
                    {
                        s = xd.Root.Element("Player").Attribute("position").Value;
                        seconds = Convert.ToInt32(s);
                    }

                    if (xd.Root.Element("Player").Attribute("playlist") != null && File.Exists(xd.Root.Element("Player").Attribute("playlist").Value))
                    {
                        s = xd.Root.Element("Player").Attribute("playlist").Value;
                        playlist = s;
                        LoadPlaylist(Playlist.Create(s));
                    }

                    if (xd.Root.Element("Design").Attribute("blackfont") != null)
                    {
                        s = xd.Root.Element("Design").Attribute("blackfont").Value;
                        cb_schwarz.IsChecked = MakeBool(s);
                    }
                    if (file != null && File.Exists(file))
                    {
                        Track t = Track.ParseFromMP3(file);
                        Tracks.Add(t);
                        track = Tracks.IndexOf(t);
                        seconds = 0;
                    }
                }        
            }
            catch (Exception ex)
            {
                track = 0;
                Tracks.Clear();
                mp.Close();
                playing = false;
                b_play.Content = "►";
                b_play.IsEnabled = false;
                InfoView("Ladefehler!" + Environment.NewLine + ex);
            }
        }
        #endregion SpeicherDaten               
        #endregion Fenster

        #region Animations
        void Start_Completed(object sender, EventArgs e)
        {
            delay.Start();
        }

        void Ausblenden_Completed(object sender, EventArgs e)
        {
            animationcomplete = true;
            i_covermini.Width = l_info.ActualHeight;
            i_covermini.Height = l_info.ActualHeight;
            l_info.Margin = new Thickness(l_info.ActualHeight + 5, 0, 0, 0);
            pb_position.Margin = new Thickness(0, l_info.ActualHeight + 10, 0, 0);
            Console.WriteLine("Ausblenden_Completed");
        }

        void Anzeigen_Completed(object sender, EventArgs e)
        {
            animationcomplete = true;
            lb_tracks.SelectedIndex = track;
            Scroll();
            Console.WriteLine("Anzeigen_Completed");
        }

        void Ausklappen_Completed(object sender, EventArgs e)
        {
            ausklappencomplete = true;
            lb_tracks.SetValue(ScrollViewer.VerticalScrollBarVisibilityProperty, ScrollBarVisibility.Auto);
            Scroll();
            Console.WriteLine("Ausklappen_Completed");
        }

        void Einklappen_Completed(object sender, EventArgs e)
        {
            ausklappencomplete = true;
            Scroll();
            Console.WriteLine("Einklappen_Completed");
        }

        void Beenden_Completed(object sender, EventArgs e)
        {
            this.Close();
        }  

        private void Scroll()
        {
            if (lb_tracks.Items.Count == Tracks.Count && Tracks.Count > track)
            {
                lb_tracks.SelectedIndex = track;
                lb_tracks.ScrollIntoView(Tracks.ElementAt(track));
            }
        }
        #endregion Animations

        private void lb_tracks_MouseEnter(object sender, MouseEventArgs e)
        {
            delay.Stop();
        }
    }    
}
