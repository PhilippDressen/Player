using Microsoft.Win32;
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

        private void sv_list_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sv_list.Height < 300)
            {
                DoubleAnimation anim = new DoubleAnimation(300, TimeSpan.FromSeconds(.2));
                sv_list.BeginAnimation(ScrollViewer.HeightProperty, anim);
            }
        }

        private void b_hintergrund_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sv_list.Height > 60)
            {
                DoubleAnimation anim = new DoubleAnimation(60, TimeSpan.FromSeconds(.2));
                sv_list.BeginAnimation(ScrollViewer.HeightProperty, anim);
            }
        }

        private void b_laden_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog 
        }
    }
}
