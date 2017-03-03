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

namespace Player.Views
{
    public partial class Info : Window
    {
        public event EventHandler OK;
        public Info()
        {
            InitializeComponent();            
        }

        public static void Show(string message)
        {
            Info i = new Info();
            i.tb_msg.Text = message;
            i.ShowActivated = true;
            i.Show();
        }

        private void b_ok_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            if (OK != null)
                OK(this, new EventArgs());
        }
    }
}
