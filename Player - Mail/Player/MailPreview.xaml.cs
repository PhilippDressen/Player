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
using System.Net.Mail;
using System.Xml.Linq;
using System.Collections.ObjectModel;

namespace Player
{
    /// <summary>
    /// Interaktionslogik für MailPreview.xaml
    /// </summary>
    public partial class MailPreview : Window
    {
        public MailPreview()
        {
            InitializeComponent();
            this.Show();
        }

        private void b_go_Click(object sender, RoutedEventArgs e)
        {
            ObservableCollection<Track> temp = new ObservableCollection<Track>((Owner as MainWindow).Tracks);
            new Thread(() =>
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("<!DOCTYPE html>");
                    sb.AppendLine("<html>");
                    sb.AppendLine("<head>");
                    sb.AppendLine("<title> Tracks Stufenfeier 2015 </title>");
                    sb.AppendLine("</head>");
                    sb.AppendLine("<body>");
                    sb.AppendLine("<h1> Playlist Stufenfeier 2015 </h1>");
                    sb.AppendLine("<h2> Tracks: " + temp.Count.ToString() + " </h2>");

                    foreach (Track track in temp)
                    {
                        sb.AppendLine("<p><b>" + track.Title + "</b> von <b>" + track.Performer + "</b> <a href=\"https://www.youtube.com/results?search_query=" + track.Title + "+" + track.Performer + "\"> Suchen </a>" + "</p>");

                        
                    }
                    sb.AppendLine("<div>");
                    sb.AppendLine("<p>Gruß</p>");
                    sb.AppendLine("<p>Philipp</p>");
                    sb.AppendLine("</div>");
                    sb.AppendLine("<div style=\"background-color:red;\">");
                    sb.AppendLine("<p>Ps: Es tut mir leid, falls das im Spamfilter hängen bleibt.</p>");
                    sb.AppendLine("</div>");
                    sb.AppendLine("</body>");
                    sb.AppendLine("</html>");

                    
                    Dispatcher.Invoke(()=>
                    {
                        Paragraph p = new Paragraph(new Run(sb.ToString()));
                        rtb.Document = new FlowDocument(p);
                    });

                    if (MessageBox.Show("SENDEN?", "Sicher?", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                        return;
                    Console.WriteLine("Mail wird gesendet...");

                    MailAddress ma = new MailAddress("matziking2@gmail.com");
                    MailMessage mm = new MailMessage("philipp@drehsen.net", "matziking2@gmail.com");
                    mm.Subject = "Musik Stufenfeier";
                    mm.IsBodyHtml = true;
                    mm.Body = sb.ToString();
                    SmtpClient smtp = new SmtpClient("outlook.office365.com");
                    smtp.EnableSsl = true;
                    smtp.Credentials = new System.Net.NetworkCredential("philipp@drehsen.net", "Leawahler365");
                    smtp.Send(mm);
                    Console.WriteLine("Mail raus!");
                }).Start();
        }
    }
}
