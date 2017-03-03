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
    /// <summary>
    /// Interaktionslogik für NeuePlaylist.xaml
    /// </summary>
    public partial class NeuePlaylist : Window
    {
        bool erstellen;
        private string _result = null;
        public string Result
        {
            get { return _result; }
            set { _result = value; }
        }

        public NeuePlaylist()
        {
            InitializeComponent();
        }

        private void Bestätigen()
        {
            erstellen = true;
            w_neueplaylist.Close();
        }

        private void b_erstellen_Click(object sender, RoutedEventArgs e)
        {
            Bestätigen();
        }

        private void tb_name_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (w_neueplaylist.IsLoaded)
            {
                if (tb_name.Text.ToLower() != "" && tb_name.Text.ToLower() != "name")
                {
                    Result = tb_name.Text;
                    b_erstellen.IsEnabled = true;
                }
                else
                {
                    Result = null;
                    b_erstellen.IsEnabled = false;
                }
            }
        }

        private void w_neueplaylist_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (erstellen == false)
            {
                Result = null;
            }
        }

        private void tb_name_GotFocus(object sender, RoutedEventArgs e)
        {
            if (tb_name.Text == "Name")
            tb_name.Text = "";
        }

        private void w_neueplaylist_Activated(object sender, EventArgs e)
        {
            tb_name.Focus();
        }

        private void tb_name_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key.Equals(Key.Return))
            {
                Bestätigen();
            }
        }

        private void b_abbrechen_Click(object sender, RoutedEventArgs e)
        {
            erstellen = false;
            this.Close();
        }
    }
}
