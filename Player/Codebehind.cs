using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Linq;
using System.Windows;
using System.Windows.Threading;
using System.Globalization;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;

namespace Player
{
    /// <summary>
    /// Businesslogik
    /// </summary>
    public class Codebehind :IDisposable
    {
        DispatcherTimer uitimer = new DispatcherTimer(DispatcherPriority.Background);
        System.Windows.Forms.NotifyIcon Tbi = new System.Windows.Forms.NotifyIcon();
        NativeMethods KH;

        public void Dispose()
        {
            //disposing Gekapselt
            if (Tbi != null)
                Tbi.Dispose();
            if (KH != null)
                KH.Dispose();
        }

        protected virtual void Dispose(bool all)
        {
            //Compilermethode
            Tbi.Dispose();
            KH.Dispose();
        }

        public Codebehind()
        {
            //erstellen der Logik & anhängen der Ereignishandler
            Prop.MediaPlayer.MediaEnded += MediaPlayer_MediaEnded;
            Prop.MediaPlayer.MediaOpened += MediaPlayer_MediaOpened;
            uitimer.Interval = TimeSpan.FromSeconds(0.5);
            uitimer.Tick += UIAktualisieren;
            Prop.OpenTracksDialog.FileOk += OpenTracksDialog_FileOk;
            App.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            try
            {
                Tbi.Icon = Properties.Resources.icon;
                Tbi.Visible = true;
                System.Windows.Forms.MenuItem mi_hide = new System.Windows.Forms.MenuItem();
                mi_hide.Text = "Verstecken";
                mi_hide.Click += mi_hide_Click;
                
                System.Windows.Forms.MenuItem mi_close = new System.Windows.Forms.MenuItem();
                mi_close.Text = "Beenden";
                mi_close.Click += mi_close_Click;
                Tbi.ContextMenu = new System.Windows.Forms.ContextMenu(new System.Windows.Forms.MenuItem[] { mi_hide , mi_close });
                Tbi.ContextMenu.Popup += (o, i)=>
                    {
                        if (Prop.View.IsVisible)
                            mi_hide.Text = "Verstecken";
                        else
                            mi_hide.Text = "Anzeigen";
                    };
                Tbi.DoubleClick += Tbi_DoubleClick;
            }
            catch (Exception ex)
            {
                Log.Write("Fehler beim erstellen des Taskbaricons!" + Environment.NewLine + ex.ToString(), EventType.Error);
            }
        }

        private void Tbi_DoubleClick(object sender, EventArgs e)
        {
            if (!Prop.View.IsVisible)
            {                
                Prop.View.Show();
            }
            Prop.View.Activate();
            Prop.View.GetAusklappen().Begin();
        }

        private void mi_close_Click(object sender, EventArgs e)
        {
            Prop.View.Close();
        }

        private void mi_hide_Click(object sender, EventArgs e)
        {
            if (Prop.View.IsVisible)
            {
                Prop.View.Hide();
            }
            else
            {
                Prop.View.Show();                
            }
        }

        void Current_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            //Unbehandelte Aushnahme loggen
            Log.Write("Unbekannter Fehler: " + Environment.NewLine + e.Exception.ToString(), EventType.Error);
            e.Handled = true;
            if (MessageBox.Show("Es ist ein unbekannter Fehler aufgetreten! (Stacktrace im Log)" + Environment.NewLine + "Soll ein anonymer Fehlerbericht an Philipp Dreßen gesendet werden?", "Fehler!", MessageBoxButton.YesNo, MessageBoxImage.Error, MessageBoxResult.Yes) == MessageBoxResult.Yes)
            {
                try
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("Stacktrace:");
                    sb.AppendLine(e.Exception.ToString());
                    sb.AppendLine("Tracks: " + Prop.Playlist.Count);
                    sb.AppendLine("OS: " + Environment.OSVersion);
                    sb.AppendLine("Cores: " + Environment.ProcessorCount);
                    sb.AppendLine("64Bit: " + Environment.Is64BitOperatingSystem);
                    sb.AppendLine("Cmd: " + Environment.CommandLine);
                    MailMessage mm = new MailMessage("philipp@drehsen.net", "philipp@drehsen.net", "Fehlerbericht Player", sb.ToString());
                    SmtpClient c = new SmtpClient("outlook.office365.com");
                    c.EnableSsl = true;
                    c.Credentials = new NetworkCredential("philipp@drehsen.net", "Leawahler365");
                    c.Send(mm);
                    Log.Write("Mail gesendet!", EventType.Success);
                    MessageBox.Show("Bericht gesendet. Danke für den Support!");
                }
                catch (Exception ex)
                {
                    Log.Write("Mail Fehler: " + Environment.NewLine + ex.ToString(), EventType.Error);
                }
            }
            if (App.Current != null)
                App.Current.Shutdown();            
        }

        void UIAktualisieren(object sender, EventArgs e)
        {
            //UI Anzeige während der Wiedergabe mit Timer updaten
            Prop.Position = Prop.MediaPlayer.Position;
            Prop.View.l_zeit.Content = DateTime.Now.ToString("dddd", CultureInfo.InstalledUICulture) + DateTime.Now.ToString(@" dd/MM/yyyy HH\:mm\:ss") + " KW: " + CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
        }

        void MediaPlayer_MediaOpened(object sender, EventArgs e)
        {
            //göffneten Track im UI Darstellen
            Track t = Prop.Playlist[Prop.Track];
            Prop.View.Titel = t.Title;
            Prop.View.Interpret = t.Performer;
            Prop.View.Album = t.Album;
            Prop.View.Cover = t.AlbumArt;
            Prop.MediaPlayer.Volume = Prop.Volume;
            string s = string.Format("{0} von {1}", t.Title, t.Performer);
            if (s.Length >= 64)
                s = s.Substring(0, 63);
            Tbi.Text = s;
            if (Prop.MediaPlayer.NaturalDuration.HasTimeSpan)
                Prop.View.Länge = Prop.MediaPlayer.NaturalDuration.TimeSpan;
            else
                Prop.View.Länge = new TimeSpan();
        }

        void MediaPlayer_MediaEnded(object sender, EventArgs e)
        {
            //nächsten Track abspielen (Bedingungen der Next() Funktion)
            Next();
        }

        public void Beenden()
        {
            //Logik herunterfahren

            //Playlist speichern, wenn Name und min 1 Track drin
            if (!(string.IsNullOrWhiteSpace(Prop.Playlistpath) && Prop.Playlist.Count == 0))
            {
                if (string.IsNullOrWhiteSpace(Prop.Playlistpath))
                {
                    //Kein Speichername für Playlist
                    if (Dialog.GetResult("Möchten Sie eine Playlist erstellen, damit die aktuelle Wiedergabe nicht verloren geht?"))
                    {
                        //User will speichern
                        String s = NeuePlaylistView.GetResult();
                        //Wenn gültiger Name angegeben speichern
                        if (!string.IsNullOrWhiteSpace(s))
                            Prop.Playlistpath = Playlist.SaveToPlaylist(Prop.Playlist, Path.Combine(Prop.PlaylistFolder, s + ".plst")).Path;
                    }
                }
                else
                {
                    //Playlistname bekannt -> Speichern
                    if (Prop.Playlist.Count > 0)
                    {
                        try
                        {
                            //Playlist schreiben
                            Playlist.SaveToPlaylist(Prop.Playlist, Prop.Playlistpath);
                        }
                        catch (Exception ex)
                        {
                            Log.Write(ex.ToString(), EventType.Error);
                            Info.Show("Fehler beim Speichern der Playlist!");
                        }
                    }
                }
            }

            try
            {
                //Speichern
                Prop.Speichern();
            }
            catch (Exception ex)
            {
                Log.Write(ex.ToString(), EventType.Error);
                Info.Show("Fehler beim Speichern!");
            }
            Dispose();
        }

        public void Bootup()
        {
            //Logik hochfahren            
            Log.Write("Anwendung gestartet. Version: " + Prop.CurrentVersion, EventType.Info);

            new Thread(EinstellungenInitialisieren).Start();            
        }

        private void EinstellungenInitialisieren()
        {
            App.Current.Dispatcher.Invoke(() => Prop.View.g_controls.IsEnabled = false);
            Prop.View.Busy = true;
            try
            {
                //Einstellungen laden
                Prop.Laden();
            }
            catch (Exception ex)
            {
                Log.Write(ex.ToString(), EventType.Error);
                App.Current.Dispatcher.Invoke(() => Info.Show("Einstellungen konnten nicht geladen werden"));
            }
            Prop.View.Dispatcher.Invoke(Prop.View.VisualBootup);

            try
            {
                LoadPlaylists();
            }
            catch (Exception ex)
            {
                Log.Write(ex.ToString(), EventType.Error);
                App.Current.Dispatcher.Invoke(() => Info.Show("Playlists konnte nicht geladen werden!"));
            }

            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                //Argumente da
                if (File.Exists(args[1]) && Path.GetExtension(args[1]) == ".plst")
                {
                    //Playlist in "Öffnen mit" -> Einstellungen
                    Log.Write(string.Format("Playlist öffnen mit \"{0}\" ...", args[1]), EventType.Info);
                    Prop.Playlistpath = args[1];
                    Prop.Track = 0;
                    Prop.Playing = true;
                    Prop.Position = new TimeSpan();
                    Prop.Muting = false;
                }
                else
                {
                    //Erstes Argument keine Playlist
                    Log.Write(string.Format("Tracks öffnen mit ({0}) ...", args.Length - 1), EventType.Info);
                    string[] files = args.Skip(1).ToArray();
                    Prop.Playlistpath = null;
                    Prop.Track = 0;
                    Prop.Playing = true;
                    Prop.Position = new TimeSpan();
                    Prop.Muting = false;
                    AddTracks(files);
                }
            }
            else
            {
                //Keine Argumente -> letzte Playlist laden, wenn da (nichts ändern)                   
            }

            if (!string.IsNullOrEmpty(Prop.Playlistpath))
            {
                try
                {
                    //Playlist in Einstellungen Laden
                    LoadPlaylist(Prop.Playlistpath);
                    Log.Write(string.Format("Playlist \"{0}\" geladen!", Prop.Playlistpath), EventType.Success);
                }
                catch (Exception ex)
                {
                    Log.Write("Playlist konnte nicht geladen werden!" + Environment.NewLine + ex.ToString(), EventType.Error);
                    App.Current.Dispatcher.Invoke(() => Info.Show(string.Format("Playlist {0} konnte nicht geladen werden!", Prop.Playlistpath)));
                    Prop.Playlistpath = null;
                }

                //Letzten Track laden, wenn da
                if (Prop.Track < Prop.Playlist.Count)
                {
                    App.Current.Dispatcher.Invoke(() => Prop.View.g_controls.IsEnabled = false);
                    App.Current.Dispatcher.Invoke(() => Prop.MediaPlayer.Open(new Uri(Prop.Playlist[Prop.Track].Path)));
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        if (Prop.Playing && Prop.Playlist.Count > 0)
                        {
                            //Abspielen wenn abspielend gespeichert und Track gültig
                            Prop.MediaPlayer.Play();
                        }
                        else
                            Prop.Playing = false;

                        //Position laden, wenn eingestellt
                        if (Prop.RememberPosition)
                            Prop.MediaPlayer.Position = Prop.Position;
                    });
                }
            }


            App.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    KH = new NativeMethods();
                    KH.KeyPressed += KH_KeyPressed;                    
                }
                catch (Exception ex)
                {
                    Log.Write("KeyHook konnte nicht initialisiert werden!" + Environment.NewLine + ex.ToString(), EventType.Error);
                    App.Current.Dispatcher.Invoke(() => Info.Show("KeyHook konnte nicht initialisiert werden!\nMöglicherweise fehlen die Rechte."));
                }
            });

            //UI aktivieren
            uitimer.Start();
            App.Current.Dispatcher.Invoke(() => Prop.View.g_controls.IsEnabled = true);
            Prop.View.Busy = false;
            Log.Write("UI-Rendering gestartet!", EventType.Success);
            Log.Write("Initialisierung abgeschlossen!", EventType.Info);
        }

        void KH_KeyPressed(ConsoleKey key)
        {
            switch (key)
            {
                case ConsoleKey.Add:
                    if (Prop.Playing)
                        Pause();
                    else
                        Play();
                    break;
                case ConsoleKey.PageUp:
                    Next();
                    break;
                case ConsoleKey.PageDown:
                    Previous();
                    break;
                case ConsoleKey.MediaPlay:
                    if (Prop.Playing)
                        Pause();
                    else
                        Play();
                    break;
                case ConsoleKey.MediaNext:
                    Next();
                    break;
                case ConsoleKey.MediaPrevious:
                    Previous();
                    break;
                case ConsoleKey.MediaStop:
                    Stop();
                    break;
            }
        }        

        public void AddTracks()
        {           
            Prop.OpenTracksDialog.ShowDialog();
        }

        void OpenTracksDialog_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Prop.View.Status = "Tracks werden geladen...";
            Prop.View.Busy = true;
            var adds = Prop.OpenTracksDialog.FileNames;
            //if (adds.Length == 0)
            //{
            //    Prop.View.Busy = false;
            //    Prop.View.Status = null;
            //    return;
            //}
            //Prop.View.g_controls.IsEnabled = false;
            //Prop.View.TracksAusklappen();
            new Thread(() =>
            {
                List<string> fails = new List<string>();
                DateTime start = DateTime.Now;
                foreach (string path in adds)
                {
                    try
                    {
                        Track t = new Track();
                        t.Path = path;
                        t.Title = Path.GetFileNameWithoutExtension(path);
                        Prop.View.Dispatcher.Invoke(() => Prop.Playlist.Add(t));
                        if (Prop.Playlist.Count == 1)
                        {
                            Prop.Track = 0;
                            App.Current.Dispatcher.Invoke(() => Prop.MediaPlayer.Open(new Uri(Prop.Playlist[Prop.Track].Path)));
                            App.Current.Dispatcher.Invoke(() => Prop.MediaPlayer.Play());
                            Prop.Playing = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Write(string.Format("Der Track {0} konnte nicht geladen werden! ({1})", path, ex.Message), EventType.Warning);
                        fails.Add(path);
                    }
                }
                Log.Write(string.Format("{0} Track(s) erfolgreich geladen. {1} Fehler. ({2}ms)", adds.Length - fails.Count, fails.Count, (DateTime.Now - start).TotalMilliseconds), EventType.Success);

                if (fails.Count < 5)
                    foreach (string s in fails)
                        App.Current.Dispatcher.Invoke(() => Info.Show(string.Format("Der Track {0} konnte nicht geladen werden.", s)));
                else
                    App.Current.Dispatcher.Invoke(() => Info.Show(string.Format("{0} Tracks konnten nicht geladen werden.", fails.Count)));
                //App.Current.Dispatcher.Invoke(Prop.NewDialog);
                //Prop.View.Dispatcher.Invoke(() => Prop.View.g_controls.IsEnabled = true);
                Prop.View.Busy = false;
                Prop.View.Tracks = Prop.Playlist.Count;
                Prop.View.Status = null;
                LoadTags();
            }).Start();
        }        

        public void AddTracks(string[] trackpaths)
        {
            if (trackpaths.Length == 0)
                return;
            Prop.View.Busy = true;
            Prop.View.Status = "Tracks werden geladen...";
            new Thread(() =>
            {
                List<string> fails = new List<string>();
                DateTime start = DateTime.Now;
                foreach (string path in trackpaths)
                {
                    try
                    {
                        Track t = new Track();
                        t.Path = path;
                        t.Title = Path.GetFileNameWithoutExtension(path);
                        Prop.View.Dispatcher.Invoke(() => Prop.Playlist.Add(t));
                    }
                    catch (Exception ex)
                    {
                        Log.Write(string.Format("Der Track {0} konnte nicht geladen werden! ({1})", path, ex.Message), EventType.Warning);
                        fails.Add(path);
                    }
                }
                Log.Write(string.Format("{0} Tracks erfolgreich geladen. {1} Fehler. ({2}ms)", trackpaths.Length - fails.Count, fails.Count, (DateTime.Now - start).TotalMilliseconds), EventType.Success);
                if (Prop.Playlist.Count == trackpaths.Length && trackpaths.Length > 0)
                {
                    Prop.Track = 0;
                    Prop.View.Dispatcher.Invoke(() => Prop.View.g_controls.IsEnabled = false);
                    App.Current.Dispatcher.Invoke(() => Prop.MediaPlayer.Open(new Uri(Prop.Playlist[Prop.Track].Path)));
                    App.Current.Dispatcher.Invoke(() => Prop.MediaPlayer.Play());
                    Prop.View.Dispatcher.Invoke(() => Prop.View.g_controls.IsEnabled = true);
                    Prop.Playing = true;
                    Prop.View.Dispatcher.Invoke(Prop.View.TracksAusklappen);
                }
                if (fails.Count < 5)
                    foreach (string s in fails)
                        App.Current.Dispatcher.Invoke(() => Info.Show(string.Format("Der Track {0} konnte nicht geladen werden.", s)));
                else
                    App.Current.Dispatcher.Invoke(() => Info.Show(string.Format("{0} Tracks konnten nicht geladen werden.", fails.Count)));
                Prop.View.Busy = false;
                Prop.View.Tracks = Prop.Playlist.Count;
                Prop.View.Status = null;
                LoadTags();
            }).Start();
        }

        public void RemoveTrack(Track t)
        {
            if (t != null && Prop.Playlist.Contains(t))
            {
                try
                {
                    int i = Prop.Playlist.IndexOf(t);
                    Prop.View.Dispatcher.Invoke(() => Prop.Playlist.Remove(t));
                    if (i < Prop.Track)
                        Prop.Track--;
                    else
                    {
                        if (i == Prop.Track)
                        {
                            SetPosition(new TimeSpan());
                            Previous();
                        }
                    }
                    Prop.View.Tracks = Prop.Playlist.Count;
                }
                catch (Exception ex)
                {
                    Log.Write(string.Format("Fehler beim Löschen vom Track \"{0}\" \n {1}", t.Path, ex), EventType.Error);
                    Prop.View.Dispatcher.Invoke(() => Info.Show("Fehler beim Löschen /n" + ex.Message));
                }
            }
        }

        public void Next()
        {
            if (Prop.Random && Prop.Playlist.Count > 0)
            {
                Prop.Track = Prop.RandomGen.Next(Prop.Playlist.Count - 1);
                App.Current.Dispatcher.Invoke(() => Prop.View.g_controls.IsEnabled = false);
                App.Current.Dispatcher.Invoke(() => Prop.MediaPlayer.Open(new Uri(Prop.Playlist[Prop.Track].Path)));
                if (Prop.Playing)
                    App.Current.Dispatcher.Invoke(() => Prop.MediaPlayer.Play());
                App.Current.Dispatcher.Invoke(() => Prop.View.g_controls.IsEnabled = true);
                return;
            }

            if (Prop.Track + 1 < Prop.Playlist.Count)
            {
                Prop.Track++;
                App.Current.Dispatcher.Invoke(() => Prop.View.g_controls.IsEnabled = false);
                App.Current.Dispatcher.Invoke(() => Prop.MediaPlayer.Open(new Uri(Prop.Playlist[Prop.Track].Path)));
                if (Prop.Playing)
                    App.Current.Dispatcher.Invoke(() => Prop.MediaPlayer.Play());
                App.Current.Dispatcher.Invoke(() => Prop.View.g_controls.IsEnabled = true);
            }
            else
            {
                if (Prop.Playlist.Count > 0 && Prop.Repeat)
                {
                    Prop.Track = 0;
                    App.Current.Dispatcher.Invoke(() => Prop.View.g_controls.IsEnabled = false);
                    App.Current.Dispatcher.Invoke(() => Prop.MediaPlayer.Open(new Uri(Prop.Playlist[Prop.Track].Path)));
                    if (Prop.Playing)
                        App.Current.Dispatcher.Invoke(() => Prop.MediaPlayer.Play());
                    App.Current.Dispatcher.Invoke(() => Prop.View.g_controls.IsEnabled = true);
                }
                else
                {
                    Prop.Playing = false;
                }
            }
        }

        public void Previous()
        {
            if (Prop.MediaPlayer.Position > TimeSpan.FromSeconds(3))
            {
                SetPosition(TimeSpan.FromSeconds(0));
                return;
            }
            if (Prop.Track - 1 >= 0)
            {
                Prop.Track--;
                App.Current.Dispatcher.Invoke(() => Prop.View.g_controls.IsEnabled = false);
                App.Current.Dispatcher.Invoke(() => Prop.MediaPlayer.Open(new Uri(Prop.Playlist[Prop.Track].Path)));
                if (Prop.Playing)
                    App.Current.Dispatcher.Invoke(() => Prop.MediaPlayer.Play());
                App.Current.Dispatcher.Invoke(() => Prop.View.g_controls.IsEnabled = true);
            }
        }

        public void Pause()
        {
            App.Current.Dispatcher.Invoke(() => Prop.MediaPlayer.Pause());
            Prop.Playing = false;
        }

        public void Stop()
        {
            App.Current.Dispatcher.Invoke(() => Prop.MediaPlayer.Stop());
            Prop.Playing = false;
        }

        public void Play()
        {
            if (App.Current.Dispatcher.Invoke<Uri>(() => Prop.MediaPlayer.Source) != null)
            {
                App.Current.Dispatcher.Invoke(() => Prop.MediaPlayer.Play());
                Prop.Playing = true;
            }
            else
                Prop.Playing = false;        
        }

        public void Play(int index)
        {
            if (Prop.Playlist.Count > index)
            {
                Prop.Track = index;
                App.Current.Dispatcher.Invoke(() => Prop.View.g_controls.IsEnabled = false);
                App.Current.Dispatcher.Invoke(() => Prop.MediaPlayer.Open(new Uri(Prop.Playlist[Prop.Track].Path)));
                App.Current.Dispatcher.Invoke(() => Prop.MediaPlayer.Play());
                App.Current.Dispatcher.Invoke(() => Prop.View.g_controls.IsEnabled = true);
                Prop.Playing = true;
            }
        }

        public void SetPosition(TimeSpan position)
        {
            App.Current.Dispatcher.Invoke(() => Prop.MediaPlayer.Position = position);
            if (Prop.Playing == false)
                App.Current.Dispatcher.Invoke(() => Prop.MediaPlayer.Pause());
            Prop.Position = position;
        }

        public void SetPan(double pan)
        {
            Prop.View.Dispatcher.Invoke(() => Prop.View.s_balance.Value = pan);
            App.Current.Dispatcher.Invoke(() => Prop.MediaPlayer.Balance = pan);
        }

        public void SearchTracks(string searchtext)
        {
            //searchtext = searchtext.ToLower();
            //if (Prop.View.lb_tracks.Visibility != Visibility.Visible)
            //    Prop.View.SlideToTracks();
            //new Thread(() =>
            //    {
            //        if (string.IsNullOrWhiteSpace(searchtext))
            //        {
            //            ResetTrackView();
            //            return;
            //        }

            //        List<Track> Ergebnisse = Prop.Playlist.Where(t =>
            //                t.Performer != null && t.Performer.ToLower().Contains(searchtext)
            //                || t.Title != null && t.Title.ToLower().Contains(searchtext)
            //                ).ToList();
            //        Prop.View.Dispatcher.Invoke(() =>
            //        { Prop.View.Trackview.Clear(); }, DispatcherPriority.Input);
            //        foreach (Track e in Ergebnisse)
            //        {
            //            Prop.View.Dispatcher.Invoke(() => { Prop.View.Trackview.Add(e); }, DispatcherPriority.Input);
            //        }
            //    }).Start();
        }        

        public void LoadPlaylist(string playlistpath)
        {
            if (!File.Exists(playlistpath))
                throw new FileLoadException("Die Datei ist nicht vorhanden!");

            Prop.View.Status = "Tracks werden geladen...";

            XDocument xd = XDocument.Load(playlistpath);
            List<string> adds = new List<string>();
            foreach (XElement xe in xd.Root.Element("body").Element("seq").Elements("media"))
            {
                adds.Add(xe.Attribute("src").Value);
            }
            
            Prop.View.Dispatcher.Invoke(()=> Prop.Playlist.Clear());            
            List<string> fails = new List<string>();
            DateTime start = DateTime.Now;

            foreach (string path in adds)
            {
                string p = path.Replace(@"..\", Directory.GetParent(Directory.GetParent(playlistpath).FullName).FullName + @"\");
                try
                {
                    Track t = new Track();
                    t.Path = p;
                    t.Title = Path.GetFileNameWithoutExtension(p);
                    if (Prop.View == null || Prop.View.Dispatcher == null || Prop.View.Dispatcher.HasShutdownStarted)
                        return;
                    Prop.View.Dispatcher.Invoke(() => Prop.Playlist.Add(t));
                }
                catch (Exception ex)
                {
                    Log.Write(string.Format("Der Track {0} konnte nicht geladen werden! ({1})", p, ex.Message), EventType.Warning);
                    fails.Add(p);
                }
            }
            Log.Write(string.Format("{0} Tracks erfolgreich geladen. {1} Fehler. ({2}ms)", adds.Count - fails.Count, fails.Count, (DateTime.Now - start).TotalMilliseconds), EventType.Success);

            if (Prop.View == null || Prop.View.Dispatcher == null || Prop.View.Dispatcher.HasShutdownStarted)
                return;
            Prop.View.Playlist = Path.GetFileNameWithoutExtension(playlistpath);
            Prop.Playlistpath = playlistpath;
            Prop.View.Tracks = Prop.Playlist.Count;
            if (fails.Count < 5)
                foreach (string s in fails)
                    App.Current.Dispatcher.Invoke(() => Info.Show(string.Format("Der Track {0} konnte nicht geladen werden.", s)));
            else
                App.Current.Dispatcher.Invoke(() => Info.Show(string.Format("{0} Tracks konnten nicht geladen werden.", fails.Count)));
            Prop.View.Status = null;
            LoadTags();
        }

        private void LoadTags()
        {
            Prop.View.Status = "ID3-Tags werden geladen...";
            DateTime start = DateTime.Now;
            new Thread(() =>
            {
                lock (Prop.Playlist)
                {
                    long Counter = 0;
                    List<Track> errors = new List<Track>();
                    for (int i = 0; i < Prop.Playlist.Count; i++)
                    {
                        Track t = Prop.Playlist[i];
                        if (!t.HasTags)
                        {
                            try
                            {
                                Track n = Track.ParseFromFile(t.Path);
                                if (Prop.View == null || Prop.View.Dispatcher == null || Prop.View.Dispatcher.HasShutdownStarted)
                                    return;
                                Prop.View.Dispatcher.Invoke(() => Prop.Playlist[i] = n);

                                if (i == Prop.Track)
                                {
                                    Prop.View.Titel = n.Title;
                                    Prop.View.Interpret = n.Performer;
                                    Prop.View.Album = n.Album;
                                    Prop.View.Cover = n.AlbumArt;
                                }
                                Counter++;
                            }
                            catch (Exception ex)
                            {
                                Log.Write(string.Format("Fehler beim Laden der Tags für {0}:{1}{2}{1}Der Track wird aus der Playlist entfernt.", t.Path, Environment.NewLine, ex.ToString()), EventType.Error);
                                errors.Add(t);
                            }
                        }
                    }
                    Log.Write(string.Format("{0} Tag(s) erfolgreich geladen! ({0}ms)", Counter, (DateTime.Now-start).TotalMilliseconds), EventType.Info);
                    foreach (Track t in errors)
                    {
                        if (Prop.View == null || Prop.View.Dispatcher == null || Prop.View.Dispatcher.HasShutdownStarted)
                            return;
                        Prop.View.Dispatcher.Invoke(() => Prop.Playlist.Remove(t));
                    }
                }
                Prop.View.Status = null;                
            }).Start();
        }

        public void LoadPlaylists()
        {
            if (!Directory.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), "Wiedergabelisten")))
                return;
            Prop.View.Status = "Playlists werden geladen...";
            IEnumerable<string> plsts = Directory.EnumerateFiles(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), "Wiedergabelisten"));
            foreach (string path in plsts)
            {
                if (Path.GetExtension(path) == ".plst" || Path.GetExtension(path) == ".zpl" || Path.GetExtension(path) == ".wpl")
                {
                    Playlist plst = Playlist.Create(path);
                    Prop.View.Dispatcher.Invoke(() => Prop.View.Playlistview.Add(plst), DispatcherPriority.Background);
                }
            }
            Prop.View.Status = null;
        }

        public void NeuePlaylist()
        {
            string s = NeuePlaylistView.GetResult();
            if (string.IsNullOrWhiteSpace(s))
            {
                return;
            }
            if (File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), "Wiedergabelisten",  s + ".plst")))
            {
                Info.Show("Playlist existiert bereits!");
                return;
            }
            Playlist plst;
            if (Prop.Playlist.Count > 0 &&  Dialog.GetResult("Möchten Sie die aktuelle Wiedergabe in die neue Playlist übernehmen?"))
                plst = Playlist.SaveToPlaylist(Prop.Playlist, Path.Combine(Prop.PlaylistFolder, s + ".plst"));
            else
                plst = Playlist.SaveToPlaylist(new List<Track>(), Path.Combine(Prop.PlaylistFolder, s + ".plst"));
            Prop.View.Playlistview.Add(plst);
        }

        public void Clear()
        {
            if (!string.IsNullOrEmpty(Prop.Playlistpath))
            {
                Playlist.SaveToPlaylist(Prop.Playlist, Prop.Playlistpath);
            }
            Prop.Track = 0;            
            Prop.Playing = false;
            Prop.Playlistpath = null;
            App.Current.Dispatcher.Invoke(() =>
                {
                    Prop.Playlist.Clear();
                    Prop.MediaPlayer.Close();
                    Prop.View.Titel = "Kein Titel";
                    Prop.View.Album = null;
                    Prop.View.Interpret = null;
                    Prop.View.i_cover.Source = null;
                    Prop.View.i_minicover.Source = null;
                    Prop.View.Playlist = null;
                    Prop.View.Tracks = 0;
                    Prop.View.Länge = new TimeSpan();
                });
        }

        public void PlaylistEntfernen(Playlist p)
        {
            if (Dialog.GetResult(string.Format("Die Playlist {0} wirklich löschen?", p.Name)))
            {
                try
                {
                    if (File.Exists(p.Path))
                        File.Delete(p.Path);
                    Prop.View.Playlistview.Remove(p);
                    if (Prop.Playlistpath == p.Path)
                        Prop.Playlistpath = null;
                }
                catch (Exception ex)
                {
                    Info.Show("Fehler beim Löschen /n" + ex.Message);
                }
            }
        }

        public void PlaylistUmbenennen(Playlist p)
        {
            try
            {
                String s = NeuePlaylistView.GetResult();
                if (string.IsNullOrWhiteSpace(s))
                    return;

                XDocument xd = XDocument.Load(p.Path);
                xd.Root.Element("head").Element("title").Value = s;
                string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), "Wiedergabelisten", s + ".plst");
                xd.Save(path);
                if (File.Exists(p.Path))
                    File.Delete(p.Path);
                Prop.View.Playlistview.Remove(p);
                Prop.View.Playlistview.Add(p);

                if (Prop.Playlistpath == p.Path)
                    Prop.Playlistpath = path;
            }
            catch (Exception ex)
            {
                Info.Show("Fehler beim Umbennenen /n" + ex.Message);
            }

        }

        public void WiedergabeInPlaylist(Playlist p)
        {
            //Aktuelle Wiedergabe einer Playlist hinzufügen
            try
            {
                XDocument xd = XDocument.Load(p.Path);
                XElement seq = xd.Root.Element("body").Element("seq");
                foreach (Track t in Prop.Playlist)
                {
                    seq.Add(
                        new XElement("media",
                            new XAttribute("src", t.Path)
                        )
                        );
                }
                xd.Save(p.Path);
                p.Length = p.Length + Prop.Playlist.Count;
                Prop.View.Playlistview.Remove(p);
                Prop.View.Playlistview.Add(p);
            }
            catch (Exception ex)
            {
                Info.Show("Fehler beim Speichern! /n" + ex.Message);
            }
        }

        /// <summary>
        /// Mit Adminrechten (und Linken) ausführen, um den Server starten zu können...
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns>Starten erfolgreich/(abgebrochen/verweigert)</returns>
        private static bool RunElevated(string fileName)
        {
            ProcessStartInfo processInfo = new ProcessStartInfo();
            processInfo.Verb = "runas";
            processInfo.FileName = fileName;
            //processInfo.Arguments = ;
            try
            {
                Process.Start(processInfo);
                return true;
            }
            catch
            {
                App.Current.Dispatcher.Invoke(()=> Info.Show("Keine ausreichenden Rechte!"));
                //Abgebrochen
            }
            return false;
        }

        public void TrackInPlaylist(Playlist p)
        {
            //Aktuellen Track einer Playlist hinzufügen
            if (Prop.Playlist.Count > Prop.Track)
            {
                try
                {
                    Track t = Prop.Playlist[Prop.Track];
                    XDocument xd = XDocument.Load(p.Path);
                    XElement seq = xd.Root.Element("body").Element("seq");
                    seq.Add(
                            new XElement("media",
                                new XAttribute("src", t.Path)
                            )
                            );
                    xd.Save(p.Path);
                    p.Length++;
                    Prop.View.Playlistview.Remove(p);
                    Prop.View.Playlistview.Add(p);
                }
                catch (Exception ex)
                {
                    Info.Show("Fehler beim Speichern! /n" + ex.Message);
                }
            }
        }

        public void ServerChange(bool start)
        {
            throw new NotImplementedException();
            //Serverstatus durch User ändern
            //if (start)
            //{
            //    if (Dialog.GetResult("Das Aktivieren der Serverfunktion benötigt möglicherweise einen Programmneustart, da der Player zum Hosten der Remotfähigkeiten immer mit Administratorrechten gestartet werden muss. Wirklich einschalten?"))
            //    {
            //        System.Security.Principal.WindowsPrincipal principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            //        if (principal.IsInRole(WindowsBuiltInRole.Administrator))
            //        {
            //            try
            //            {
            //                Prop.Server = WebApp.Start("http://*:4000");
            //                Log.Write("Server gestartet!", EventType.Success);
            //                Prop.EnableServer = true;
            //            }
            //            catch (Exception ex)
            //            {
            //                Log.Write(ex.ToString(), EventType.Error);
            //                Prop.EnableServer = false;
            //                Info.Show("Fehler beim Starten des Servers!");
            //            }
            //        }
            //        else
            //        {
            //            Prop.EnableServer = true;
            //            //Prop.Speichern();
            //            Prop.View.Close();
            //            ProcessStartInfo psi = new ProcessStartInfo(Assembly.GetExecutingAssembly().CodeBase);
            //            Process.Start(psi);
            //        }
            //    }
            //    else
            //        Prop.EnableServer = false;
            //}
            //else
            //{
            //    if (Prop.Server != null)
            //    {
            //        try
            //        {
            //            Prop.Server.Dispose();
            //        }
            //        catch { }
            //        Prop.Server = null;

            //    }
            //    Prop.EnableServer = false;
            //}            
        }

        public void OpenImage()
        {
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.Filter = "Bilder|*.jpg; *.png; *.gif";
            ofd.Title = "Hintergrundbild auswählen...";
            ofd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            ofd.FileOk+=ofd_FileOk;
            ofd.ShowDialog();
        }

        private void ofd_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            new Thread(() =>
            {
                try
                {
                    Prop.BackgroundImage = (sender as Microsoft.Win32.OpenFileDialog).FileName;
                }
                catch (Exception ex)
                {
                    Log.Write("Fehler beim Setzen des Hintergrunds: " + Environment.NewLine + ex.ToString(), EventType.Error);
                    Info.Show("Fehler beim Laden des Hintergrunds!");
                }
            }).Start();            
        }

        public void NachOben(Track t)
        {
            if (t != null && Prop.Playlist.Contains(t))
            {
                int index = Prop.Playlist.IndexOf(t);
                if (index > 0)
                {
                    Prop.Playlist.Move(index, index-1);
                    if (Prop.Track == index)
                    {
                        Prop.Track--;
                    }
                    else if (Prop.Track == index-1)
                    {
                        Prop.Track = index;
                    }
                }
            }
        }

        public void NachUnten(Track t)
        {
            if (t != null && Prop.Playlist.Contains(t))
            {
                int index = Prop.Playlist.IndexOf(t);
                if (index < Prop.Playlist.Count)
                {
                    Prop.Playlist.Move(index, index + 1);
                    if (Prop.Track == index)
                    {
                        Prop.Track++;
                    }
                    else if (Prop.Track == index + 1)
                    {
                        Prop.Track = index;
                    }
                }
            }
        }

        public void VonVorn()
        {
            if (Prop.Playlist.Count > 0)
            {
                Play(0);
            }
        }

        public void EditTrack(Track t)
        {
            if (t != null)
            {
                TagEditView.Show(t);
            }
        }
    }
}
