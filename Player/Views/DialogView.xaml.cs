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

namespace Player
{
    public partial class Dialog : Window
    {
        private bool _result = false;
        public bool Result
        {
            get { return _result; }
            set { _result = value; }
        }

        public Dialog()
        {
            InitializeComponent();            
        }

        public static bool GetResult(string message)
        {
            Dialog d = new Dialog();
            d.tb_msg.Text = message;
            d.ShowDialog();
            return d.Result;
        }
        
        private void b_ja_Click(object sender, RoutedEventArgs e)
        {
            Result = true;
            this.Close();
        }

        private void b_nein_Click(object sender, RoutedEventArgs e)
        {
            Result = false;
            this.Close();
        }
    }
}
