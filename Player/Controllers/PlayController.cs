using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using System.Windows.Media;
using System.Diagnostics;
using System.Windows;
using System.Xml.Linq;
using System.IO;
using Microsoft.Win32;
using System.Reflection;
using System.Media;
using System.Windows.Media.Imaging;
using System.Threading;
using System.Deployment.Application;
using Player.Views;


namespace Player.Controllers
{
    /// <summary>
    /// statische Ressourcen
    /// </summary>
    class PlayController
    {
        public static string CurrentVersion
        {
            get
            {
                return ApplicationDeployment.IsNetworkDeployed
                       ? ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString()
                       : Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }   

        public static String PlaylistFolder
        {
            get
            {
                return System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), "Wiedergabelisten");
            }
        }

        public static String AppFolder
        {
            get
            {
                string p = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Player");
                if (!Directory.Exists(p))
                    Directory.CreateDirectory(p);
                return p;
            }
        }

        static MediaPlayer _mediaplayer;
        public static MediaPlayer MediaPlayer
        {
            get
            {
                if (_mediaplayer == null)
                    App.Current.Dispatcher.Invoke(()=>
                        { _mediaplayer = new MediaPlayer();
                        });
                return _mediaplayer; }
        }

        static ObservableCollection<Track> _playlist = new ObservableCollection<Track>();
        public static ObservableCollection<Track> Playlist
        {
            get
            { return _playlist;   }
        }

        static MainWindowView _view;
        public static MainWindowView View
        {
            get
            {
                return _view;
            }
            set
            {
                _view = value;
            }
        }

        static OpenFileDialog _ofd = new OpenFileDialog() { Title = "Titel laden...", Multiselect = true, Filter = "Medien-Dateien|*.mp3; *.wav; *.wmv; *.wma; *.m3u; *.mp4"};
        public static OpenFileDialog OpenTracksDialog
        {
            get { return _ofd; }
        }

        static int _track;
        public static int Track
        {
            get { return _track; }
            set
            {
                _track = value;
                if (View != null)
                {
                    View.Dispatcher.Invoke(() =>
                    {
                        View.Track = value;
                        if (View.lb_tracks.Items.Count > value)
                        {
                            View.lb_tracks.SelectedIndex = value;
                            View.lb_tracks.ScrollIntoView(PlayController.Playlist[value]);
                        }
                    });
                }
            }
        }

        static string _playlistpath;
        public static string Playlistpath
        {
            get { return _playlistpath; }
            set
            {
                _playlistpath = value;
            }
        }

        static bool _playing;
        public static bool Playing
        {
            get { return _playing; }
            set
            {
                _playing = value;
                if (View != null)
                {
                    if (value)
                        View.Dispatcher.Invoke(() => View.b_play.Content="II");
                    else
                        View.Dispatcher.Invoke(() => View.b_play.Content = " ► ");
                }
            }
        }

        static string _backgroundimage;
        public static string BackgroundImage
        {
            get
            {
                return _backgroundimage;
            }
            set
            {
                if (value == null)
                {
                    if (View != null)
                    {
                        View.Dispatcher.Invoke(() => View.b_hintergrund.Background = new SolidColorBrush(Colors.Black) { Opacity = 0.8 });
                    }
                    _backgroundimage = value;
                    return;
                }
                if (File.Exists(value))
                {
                    if (View != null)
                    {
                        BitmapImage bi = new BitmapImage(new Uri(value));
                        bi.Freeze();
                        View.Dispatcher.Invoke(() =>
                           {
                               View.b_hintergrund.Background = new ImageBrush(bi) { Stretch = Stretch.UniformToFill, Opacity = 0.8 };
                           });
                    }
                    _backgroundimage = value;
                }
            }
        }

        static bool _muting;
        public static bool Muting
        {
            get { return _muting; }
            set
            {
                _muting = value;
                if (View != null)
                {
                    View.Dispatcher.Invoke(() => View.cb_mute.IsChecked = value);
                    App.Current.Dispatcher.Invoke(() => MediaPlayer.IsMuted = value);
                }
            }
        }

        static bool _random;
        public static bool Random
        {
            get { return _random; }
            set
            {
                _random = value;
                if (View != null)
                {
                    View.Dispatcher.Invoke(() => View.cb_zufall.IsChecked = value);
                }
            }
        }

        static bool _repeat;
        public static bool Repeat
        {
            get { return _repeat; }
            set
            {
                _repeat = value;
                if (View != null)
                {
                    View.Dispatcher.Invoke(() => View.cb_wiederholen.IsChecked = value);
                }
            }
        }

        static bool _rememberposition;
        public static bool RememberPosition
        {
            get { return _rememberposition; }
            set
            {
                _rememberposition = value;
                if (View != null)
                {
                    View.Dispatcher.Invoke(() => View.cb_positonmerken.IsChecked = value);
                }
            }
        }

        static double _volume = 0.8;
        public static double Volume
        {
            get { return _volume; }
            set
            {
                if (value >= 0 && value <= 1)
                {
                    _volume = value;
                    App.Current.Dispatcher.Invoke(() => MediaPlayer.Volume = value);
                    if (View != null)
                    {
                        View.Dispatcher.Invoke(() => View.s_lautstärke.Value = value * 100);
                    }
                }
            }
        }

        static double _speed = 1;
        public static double Speed
        {
            get { return _speed; }
            set
            {
                if (value >= 0.5 && value <=2)
                {
                    _speed = value;
                    if (View != null)
                    {
                        View.Dispatcher.Invoke(() =>
                        {
                            View.s_speed.Value = value;
                            View.l_speed.Content = string.Format("Geschwindigkeit: {0}x", value.ToString("0.00"));
                        });
                    }
                    App.Current.Dispatcher.Invoke(() => MediaPlayer.SpeedRatio = value);
                }
            }
        }

        static Point _wposition;
        public static Point WindowPosition
        {
            get { return _wposition; }
            set
            {
                if (value.X < System.Windows.SystemParameters.PrimaryScreenWidth && value.X >= 0
                    && value.Y < System.Windows.SystemParameters.PrimaryScreenHeight && value.Y >= 0)
                {
                    _wposition = value;
                    if (View != null && View.Dispatcher.Invoke<bool>(()=> View.Left != value.X || View.Top != value.Y) )
                    {
                        View.Dispatcher.Invoke(() =>
                        {
                            View.Top = value.Y;
                            View.Left = value.X;
                        });
                    }
                }
            }
        }

        static TimeSpan _position = new TimeSpan();
        public static TimeSpan Position
        {
            get { return _position; }
            set
            {
                _position = value;
                if (View != null)
                {
                    View.Dispatcher.Invoke(() => View.Position = value);
                }
            }
        }

        static bool _disableanim;
        public static bool DisableAnimations
        {
            get { return _disableanim; }
            set
            {
                _disableanim = value;
                if (View != null)
                {
                    View.Dispatcher.Invoke(() => View.cb_animationen.IsChecked = value);
                }
            }
        }

        static Random r = new Random(DateTime.Now.Second);
        public static Random RandomGen
        {
            get { return r; }
        }

        static IDisposable _server;
        public static IDisposable Server
        {
            get { return _server; }
            set { _server = value; }
        }

        static string savepath = Path.Combine(AppFolder, "save.cfg");
        public static void Speichern()
        {
            DateTime start = DateTime.Now;
            try
            {
                string savedir = Path.GetDirectoryName(savepath);
                Directory.CreateDirectory(savedir);
            }
            catch (Exception ex)
            {
                Log.Write(ex.ToString(), EventType.Error);
                App.Current.Dispatcher.Invoke(() => Info.Show("Programmordner konnte nicht erstellt werden!"));
            } 
            
            XDocument xd = new XDocument();
            xd.AddFirst(
                new XElement("Configuration",
                    new XAttribute("Version", CurrentVersion),
                    new XAttribute("Saved", DateTime.Now.ToString())
                    )
            );
            xd.Root.Add(new XElement("Playing", new XAttribute("value", Playing)));
            xd.Root.Add(new XElement("Muting", new XAttribute("value", Muting)));
            xd.Root.Add(new XElement("Repeat", new XAttribute("value", Repeat)));
            xd.Root.Add(new XElement("Random", new XAttribute("value", Random)));
            xd.Root.Add(new XElement("RememberPosition", new XAttribute("value", RememberPosition)));
            xd.Root.Add(new XElement("EnableServer", new XAttribute("value", false)));
            if (Playlistpath != null)
                xd.Root.Add(new XElement("Playlist", new XAttribute("value", Playlistpath)));
            xd.Root.Add(new XElement("Track", new XAttribute("value", Track.ToString())));
            xd.Root.Add(new XElement("Position", new XAttribute("value", App.Current.Dispatcher.Invoke<TimeSpan>(() => MediaPlayer.Position).ToString())));
            xd.Root.Add(new XElement("Volume", new XAttribute("value", Volume.ToString())));            
            xd.Root.Add(new XElement("Speed", new XAttribute("value", Speed.ToString())));
            Point p =  Application.Current.Dispatcher.Invoke(() => new Point(View.Left, View.Top));
            xd.Root.Add( new XElement("WindowPosition", new XAttribute("value", p.ToString(CultureInfo.GetCultureInfo(1)))) );
            if (BackgroundImage != null)
                xd.Root.Add(new XElement("BackgroundImage", new XAttribute("value", BackgroundImage)));
            xd.Root.Add(new XElement("DisableAnimations", new XAttribute("value", DisableAnimations)));

            xd.Save(savepath);
            Log.Write(string.Format("Einstellungen gespeichert. ({0}ms)", (DateTime.Now - start).TotalMilliseconds), EventType.Success);
            if (Log.CanLog)
                Log.LogWriter.Close();
        }

        public static void Laden()
        {
            if (File.Exists(savepath))
            {
                PlayController.View.Status = "Einstellungen werden geladen...";
                DateTime start = DateTime.Now;
                XDocument xd = XDocument.Load(savepath);
                Playing = Convert.ToBoolean(xd.Root.Element("Playing").Attribute("value").Value);
                Muting = Convert.ToBoolean(xd.Root.Element("Muting").Attribute("value").Value);
                Repeat = Convert.ToBoolean(xd.Root.Element("Repeat").Attribute("value").Value);
                Random = Convert.ToBoolean(xd.Root.Element("Random").Attribute("value").Value);
                RememberPosition = Convert.ToBoolean(xd.Root.Element("RememberPosition").Attribute("value").Value);
                //EnableServer = Convert.ToBoolean(xd.Root.Element("EnableServer").Attribute("value").Value);
                if (xd.Root.Element("Playlist") != null && xd.Root.Element("Playlist").Attribute("value") != null)
                    Playlistpath = xd.Root.Element("Playlist").Attribute("value").Value;
                Track = Convert.ToInt32(xd.Root.Element("Track").Attribute("value").Value);
                Position = TimeSpan.Parse(xd.Root.Element("Position").Attribute("value").Value);
                Volume = Convert.ToDouble(xd.Root.Element("Volume").Attribute("value").Value);
                Speed = Convert.ToDouble(xd.Root.Element("Speed").Attribute("value").Value);
                if (xd.Root.Element("WindowPosition") != null)
                {
                    WindowPosition = Point.Parse(xd.Root.Element("WindowPosition").Attribute("value").Value);
                }
                if (xd.Root.Element("BackgroundImage") != null)
                {

                    new Thread(() =>
                        {
                            try
                            {
                                BackgroundImage = xd.Root.Element("BackgroundImage").Attribute("value").Value;
                            }
                            catch
                            {
                                Info.Show("Fehler beim Laden des Hintergrunds!");
                            }
                        }).Start();
                }
                if (xd.Root.Element("DisableAnimations") != null)
                {
                    DisableAnimations = Convert.ToBoolean(xd.Root.Element("DisableAnimations").Attribute("value").Value);
                }
                Log.Write(string.Format("Einstellungen geladen. ({0}ms)", (DateTime.Now - start).TotalMilliseconds), EventType.Success);
                //if (xd.Root.Attribute("Version") == null || xd.Root.Attribute("Version").Value != CurrentVersion)
                //    View.Dispatcher.Invoke(ShowChangeLog);
                PlayController.View.Status = null;
            }
            else
            {
                Log.Write("Kein Einstellungen gefunden.", EventType.Info);
            }

            PlayController.View.Status = null;
        }

        private static void ShowChangeLog()
        {
            string logtext = Player.Properties.Resources.ChangeLog;
            Info.Show(logtext);
            Log.Write("ChangeLog angezeigt.", EventType.Info);
        }

        private static bool RunElevated(string fileName)
        {
            ProcessStartInfo processInfo = new ProcessStartInfo();
            processInfo.Verb = "runas";
            processInfo.FileName = fileName;
            try
            {
                Process.Start(processInfo);
                return true;
            }
            catch
            {
                MessageBox.Show("Keine ausreichenden Rechte!");
                //Abbrechen
            }
            return false;
        }
    }
}
