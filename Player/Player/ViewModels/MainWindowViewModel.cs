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

        private NativeMethods natives;
        #endregion

        public MainWindowViewModel()
        {
            TaskbarIcon = new System.Windows.Forms.NotifyIcon();
            SetupTaskbarIcon();
            LoadSettings().Wait();
            natives = new NativeMethods();
            natives.KeyPressed += Natives_KeyPressed;
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
                //case ConsoleKey.MediaStop:
                //    Stop();
                //    break;
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
            Task.Run(SaveSettings);
            TaskbarIcon.Dispose();
        }
        
        private async Task LoadSettings()
        {
            Status = "Einstellungen laden...";
            Settings set = Settings.Default;
            Left = set.WindowPosition.X;
            Top = set.WindowPosition.Y;
            Status = null;
        }

        private async Task SaveSettings()
        {
            Status = "Einstellungen speichern...";
            Settings set = Settings.Default;
            set.WindowPosition = new Point(Left, Top);
            //set.
            set.Save();
            Status = null;
        }

        public void Play()
        {

        }

        public void Pause()
        {

        }

        public void Next()
        {

        }

        public void Previous()
        {

        }
    }
}