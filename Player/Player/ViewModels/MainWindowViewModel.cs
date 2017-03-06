using Player.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Deployment.Application;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using Player.Helpers;
using Player.Models;
using System.Reflection;

namespace Player.ViewModels
{
    class MainWindowViewModel : INotifyPropertyChanged
    {
        #region Properties
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName]string name = null)
        {
            App.Current.Dispatcher.Invoke(() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)));
        }

        static MediaPlayer mediaplayer;
        public static MediaPlayer MediaPlayer
        {
            get
            {
                if (mediaplayer == null)
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        mediaplayer = new MediaPlayer();
                    });
                return mediaplayer;
            }
        }

        private bool isplaying = false;
        public bool IsPlaying
        {
            get
            {
                return isplaying;
            }
            set
            {
                isplaying = value;
                NotifyPropertyChanged();
            }
        }

        private bool ismuting = true;
        public bool IsMuting
        {
            get
            {
                return ismuting;
            }
            set
            {
                ismuting = value;
                NotifyPropertyChanged();
            }
        }

        private bool repeat = true;
        public bool Repeat
        {
            get
            {
                return repeat;
            }
            set
            {
                repeat = value;
                NotifyPropertyChanged();
            }
        }


        private bool random = true;
        public bool Random
        {
            get
            {
                return random;
            }
            set
            {
                random = value;
                NotifyPropertyChanged();
            }
        }

        private bool rememberposition = false;
        public bool RememberPosition
        {
            get
            {
                return rememberposition;
            }
            set
            {
                rememberposition = value;
                NotifyPropertyChanged();
            }
        }

        public static Version CurrentVersion
        {
            get
            {
                return ApplicationDeployment.IsNetworkDeployed
                       ? ApplicationDeployment.CurrentDeployment.CurrentVersion
                       : Assembly.GetExecutingAssembly().GetName().Version;
            }
        }

        public static string PlaylistFolder
        {
            get
            {
                return System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), "Wiedergabelisten");
            }
        }

        public static string PlaylistPath
        {
            get
            {
                return System.IO.Path.Combine(PlaylistFolder, "Playlist.plst");
            }
        }

        public static string AppFolder
        {
            get
            {
                string p = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Player");
                if (!Directory.Exists(p))
                    Directory.CreateDirectory(p);
                return p;
            }
        }

        public System.Windows.Forms.NotifyIcon TaskbarIcon
        { get; }

        private string title = "Player";
        public string Title
        {
            get
            {
                return title;
            }
            set
            {
                title = value;
                NotifyPropertyChanged();
            }
        }

        private bool isenabled = true;
        public bool IsEnabled
        {
            get
            {
                return isenabled;
            }
            set
            {
                isenabled = value;
                NotifyPropertyChanged();
            }
        }

        private bool stayopen = false;
        public bool StayOpen
        {
            get
            {
                return stayopen;
            }
            set
            {
                stayopen = value;
                NotifyPropertyChanged();
            }
        }

        private int top = 0;
        public int Top
        {
            get
            {
                return top;
            }
            set
            {
                top = value;
                NotifyPropertyChanged();
            }
        }

        private int left = 0;
        public int Left
        {
            get
            {
                return left;
            }
            set
            {
                left = value;
                NotifyPropertyChanged();
            }
        }

        private string status;
        public string Status
        {
            get
            {
                return status;
            }
            set
            {
                status = value;
                NotifyPropertyChanged();
            }
        }

        private TimeSpan duration;
        public TimeSpan Duration
        {
            get
            {
                return duration;
            }
            set
            {
                duration = value;
                NotifyPropertyChanged();
            }
        }

        private TimeSpan position;
        public TimeSpan Position
        {
            get
            {
                return position;
            }
            set
            {
                position = value;
                NotifyPropertyChanged();
            }
        }

        public double Volume
        {
            get
            {
                return MediaPlayer.Volume * 100;
            }
            set
            {
                MediaPlayer.Volume = value/100;
                NotifyPropertyChanged();
            }
        }

        private Playlist playlist;
        public Playlist Playlist
        {
            get
            {
                return playlist;
            }
            set
            {
                playlist = value;
                NotifyPropertyChanged();
            }
        }

        private Track currenttrack;
        public Track CurrentTrack
        {
            get
            {
                return currenttrack;
            }
            set
            {
                currenttrack = value;
                NotifyPropertyChanged();
            }
        }

        private NativeMethods natives;
        #endregion

        public MainWindowViewModel()
        {
            TaskbarIcon = new System.Windows.Forms.NotifyIcon();
            SetupTaskbarIcon();
            LoadSettings();
            try
            {
                natives = new NativeMethods();
                natives.KeyPressed += Natives_KeyPressed;
            }
            catch (Exception ex)
            {
                Log.Write("Error registering KeyHooks" + Environment.NewLine + ex, EventType.Error);
            }
            MediaPlayer.MediaOpened += MediaPlayer_MediaOpened;
            MediaPlayer.MediaEnded += MediaPlayer_MediaEnded;
            MediaPlayer.MediaFailed += MediaPlayer_MediaFailed;
        }

        private void MediaPlayer_MediaFailed(object sender, ExceptionEventArgs e)
        {
            Log.Write("Error opening media" + Environment.NewLine + e.ErrorException, EventType.Error);
            Status = "Öffnen fehlgeschlagen...";
            Next();
        }

        private void MediaPlayer_MediaEnded(object sender, EventArgs e)
        {
            Next();
        }

        private void MediaPlayer_MediaOpened(object sender, EventArgs e)
        {
            if (IsPlaying)
            {
                MediaPlayer.Play();
            }
            Status = null;
        }

        private void Natives_KeyPressed(ConsoleKey key)
        {
            switch (key)
            {
                case ConsoleKey.Add:
                    if (IsPlaying)
                        Pause();
                    else
                        Play();
                    break;
                case ConsoleKey.PageUp:
                    Next();
                    break;
                case ConsoleKey.PageDown:
                    Previous();
                    break;
                case ConsoleKey.MediaPlay:
                    if (IsPlaying)
                        Pause();
                    else
                        Play();
                    break;
                case ConsoleKey.MediaNext:
                    Next();
                    break;
                case ConsoleKey.MediaPrevious:
                    Previous();
                    break;
                case ConsoleKey.MediaStop:
                    Stop();
                    break;
            }
        }

        private void SetupTaskbarIcon()
        {
            TaskbarIcon.Icon = new Icon("Icons/iconmini.ico");
            TaskbarIcon.Text = Title;
            TaskbarIcon.Visible = true;
            TaskbarIcon.MouseDoubleClick += TaskbarIcon_MouseDoubleClick;
            System.Windows.Forms.MenuItem mi_close = new System.Windows.Forms.MenuItem("Beenden");
            mi_close.Click += Mi_close_Click;
            System.Windows.Forms.MenuItem[] menuitems = new System.Windows.Forms.MenuItem[] { mi_close };
            TaskbarIcon.ContextMenu = new System.Windows.Forms.ContextMenu(menuitems);
        }

        private void Mi_close_Click(object sender, EventArgs e)
        {
            Close();
            App.Current.Shutdown(0);
        }

        private void TaskbarIcon_MouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            TaskbarIcon.ShowBalloonTip(3000, Title, "This is Music!", System.Windows.Forms.ToolTipIcon.Info);
        }

        public void Close()
        {
            try
            {
                SaveSettings();
            }
            catch (Exception ex)
            {
                Log.Write("Error saving settings" + Environment.NewLine + ex, EventType.Error);
            }

            try
            {
                this.Playlist?.Save(PlaylistPath);
            }
            catch (Exception ex)
            {
                Log.Write("Error saving Playlist" + Environment.NewLine + ex, EventType.Error);
            }
            TaskbarIcon.Dispose();
        }
        
        private async Task LoadSettings()
        {
            Status = "Einstellungen laden...";
            Settings set = Settings.Default;
            Left = set.WindowPosition.X;
            Top = set.WindowPosition.Y;
            IsMuting = set.Muting;
            IsPlaying = set.Playing;
            Random = set.Random;
            Repeat = set.Repeat;
            Volume = set.Volume;
            Playlist = Playlist.Load(set.Playlist);
            LoadTrack(set.Track);
            MediaPlayer.SpeedRatio = set.SpeedRatio;
            MediaPlayer.Position = set.Position;
            Status = null;
        }

        private async Task SaveSettings()
        {
            Status = "Einstellungen speichern...";
            Settings set = Settings.Default;            
            set.Muting = IsMuting;
            set.Playing = IsPlaying;
            set.Playlist = Playlist.Path;
            set.Random = Random;
            set.SpeedRatio = MediaPlayer.SpeedRatio;
            set.Volume = Volume;
            set.Track = Playlist.Tracks.IndexOf(CurrentTrack);
            set.Repeat = Repeat;
            set.WindowPosition = new Point(Left, Top);
            if (RememberPosition)
            {
                set.Position = MediaPlayer.Position;
            }
            else
            {
                set.Position = new TimeSpan();
            }
            set.Save();
            Status = null;
        }

        public void AddTracks(IEnumerable<string> paths)
        {
            Task.Factory.StartNew(() =>
            {
                Status = "Loading Tracks...";
                if (this.Playlist == null)
                {
                    this.Playlist = new Playlist(PlaylistPath);
                }
                foreach (string path in paths)
                {
                    try
                    {
                        Track t = Track.ParseFromFile(path); //new Track(path);
                        App.Current.Dispatcher.Invoke(() => this.Playlist.Tracks.Add(t), System.Windows.Threading.DispatcherPriority.DataBind);
                    }
                    catch (Exception ex)
                    {
                        Log.Write("Error loading Tags for " + path + Environment.NewLine + ex, EventType.Error);
                    }
                }
                Status = null;
            });
        }

        public void LoadTrack(int i)
        {
            CurrentTrack = Playlist.Tracks.ElementAt(i);
            Status = "Öffne Datei...";
            MediaPlayer.Open(new Uri(CurrentTrack.Path));
        }

        public void Play()
        {
            MediaPlayer.Play();
            IsPlaying = true;
        }

        public void Pause()
        {
            MediaPlayer.Pause();
            IsPlaying = false;
        }

        public void Stop()
        {
            MediaPlayer.Stop();
            IsPlaying = false;
        }

        public void Next()
        {

        }

        public void Previous()
        {

        }
    }
}