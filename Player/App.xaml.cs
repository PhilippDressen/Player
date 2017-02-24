using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Security.Principal;
using Player.Controllers;

namespace Player
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application
    {
        protected override void OnNavigating(System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            var p = Process.GetProcessesByName("Player");
            if (p.Length > 1)
            {
                //Log.Write("Anwendung ist bereits gestartet.", EventType.Warning);
                e.Cancel = true;
                this.Shutdown();
            }
            
            base.OnNavigating(e);
        }

        private void StackPanel_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(Track)) && (e.Data.GetData(typeof(Track))) != (((e.Source as FrameworkElement).DataContext) as Player.Track))
            {
                Console.WriteLine( (e.Data.GetData(typeof(Track))).ToString() + " nach:" );
                Console.WriteLine((((e.Source as FrameworkElement).DataContext) as Track).ToString());
                try
                {
                    int from = Prop.Playlist.IndexOf((e.Data.GetData(typeof(Track))) as Track);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }

    }
}
