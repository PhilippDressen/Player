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
using Player.Views;


namespace Player.Views
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
        
    }
}
