using Player.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Player.ViewModels
{
    class MainWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName]string name = null)
        {
            App.Current.Dispatcher.Invoke(() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)));
        }

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

        public MainWindowViewModel()
        {
            LoadSettings();
        }   

        public void Close()
        {
            SaveSettings();
        }
        
        private void LoadSettings()
        {
            Settings set = Settings.Default;
            Left = set.WindowPosition.X;
            Top = set.WindowPosition.Y;
        }

        private void SaveSettings()
        {
            Settings set = Settings.Default;
            set.WindowPosition = new Point(Left, Top);
            set.Save();
        }
    }
}