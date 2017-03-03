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
using System.Windows.Shapes;
using System.Threading;
using Player.Views;

namespace Player.Views
{
    public partial class Laden : Window
    {
        private int _status = 0;
        public int Status
        {
            get {   return _status; }
            set {
                    _status = value;
                    Dispatcher.Invoke(new ThreadStart(() =>
                    {
                            l_msg.Content = "Track " + Status + " von " + Max + " wird geladen...";
                        pb_status.Value = _status;
                        l_msg.UpdateLayout();
                        w_laden.Width = double.NaN;
                        w_laden.UpdateLayout();                        
                    }));                    
                }
        }

        private int _max = 0;
        public int Max
        {
            get { return _max; }
            set
            {
                _max = value;
                Dispatcher.Invoke(new ThreadStart(() =>
                {
                        pb_status.Maximum = _max;
                }));
            }
        }

        public Laden()
        {
            InitializeComponent();
        }

        private void b_abbrechen_Click(object sender, RoutedEventArgs e)
        {
            MainWindow parent = this.Owner as MainWindow;
            if (parent != null)
            {
                //parent.Abbrechen = true;
                //To Do: Cencellation            
            }
            this.Close();
        }
    }
}
