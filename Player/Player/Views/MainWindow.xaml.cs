using Microsoft.Win32;
using Player.Models;
using Player.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Player.Views
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        static OpenFileDialog _ofd = new OpenFileDialog() { Title = "Titel laden...", Multiselect = true, Filter = "Medien-Dateien|*.mp3; *.wav; *.wmv; *.wma; *.m3u; *.mp4" };
        public static OpenFileDialog OpenTracksDialog
        {
            get { return _ofd; }
        }

        private MainWindowViewModel ViewModel
        {
            get
            {
                return this.DataContext as MainWindowViewModel;
            }
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void b_close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void w_mainwindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void w_mainwindow_Closed(object sender, EventArgs e)
        {
            ViewModel.Close();
        }

        private void b_laden_Click(object sender, RoutedEventArgs e)
        {
            OpenTracksDialog.ShowDialog(this);
            string[] paths = new string[OpenTracksDialog.FileNames.Length];
            OpenTracksDialog.FileNames.CopyTo(paths, 0);
            if (paths.Length > 0)
            {
                ViewModel.AddTracks(paths);
            }
        }

        private void lb_tracks_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lb_tracks.SelectedItem != null)
            {
                ViewModel.LoadTrack(lb_tracks.SelectedIndex);
                MinimizeList();
            }
        }

        private const double t_dropout = .1;
        private void sv_list_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            MaximizeList();    
        }

        private void b_hintergrund_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MinimizeList();
        }

        private void MaximizeList()
        {
            if (sv_list.Height < 300)
            {
                DoubleAnimation anim = new DoubleAnimation(300, TimeSpan.FromSeconds(t_dropout));
                sv_list.BeginAnimation(ScrollViewer.HeightProperty, anim);
            }
        }

        private void MinimizeList()
        {
            if (sv_list.Height > 60)
            {
                DoubleAnimation anim = new DoubleAnimation(60, TimeSpan.FromSeconds(t_dropout));
                sv_list.BeginAnimation(ScrollViewer.HeightProperty, anim);
            }
        }

        private void b_play_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.IsPlaying)
            {
                ViewModel.Pause();
            } else
            {
                ViewModel.Play();
            }
        }

        private void s_volume_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ViewModel.Volume = 80;
        }
    }
}
