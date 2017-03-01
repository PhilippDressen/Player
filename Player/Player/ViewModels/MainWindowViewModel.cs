using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Player.ViewModels
{
    class MainWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName]string name = null)
        {
            App.Current.Dispatcher.Invoke(() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)));
        }

        private string title = "Title";
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

        public MainWindowViewModel()
        {

        }        
    }
}