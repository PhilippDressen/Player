using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using Player.ViewModels;
using Player.Controllers;

namespace Player.Views
{
    public enum State
    {
        Collapsed,
        Expanded,
        Collapsing,
        Expanding
    }

    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindowView : Window
    {
        Codebehind _logic;
        public Codebehind Logic
        { get { return _logic; } set { _logic = value; } }

        ObservableCollection<Playlist> _playlistview = new ObservableCollection<Playlist>();
        public ObservableCollection<Playlist> Playlistview
        { get { return _playlistview; } }

        DispatcherTimer minitimer = new DispatcherTimer(DispatcherPriority.Normal);
        Storyboard Einklappen;
        Storyboard Ausklappen;
        State playerstate = State.Expanded;

        public MainWindowView()
        {
            InitializeComponent();
            minitimer.Interval = TimeSpan.FromSeconds(3);
            minitimer.Tick += Minitimer_Tick;
        }

        private void w_player_Initialized(object sender, EventArgs e)
        {
            lb_tracks.ItemsSource = Prop.Playlist;
            lb_playlists.ItemsSource = Playlistview;
            Prop.View = this;
            Logic = new Codebehind();
            Logic.Bootup();
        }

        public void VisualBootup()
        {
            b_hintergrund.RenderTransform = new ScaleTransform();
            Storyboard start = new Storyboard();
            DoubleAnimationUsingKeyFrames Anim = new DoubleAnimationUsingKeyFrames();
            BounceEase f = new BounceEase();
            f.Bounciness = 5;
            Anim.KeyFrames.Add(new EasingDoubleKeyFrame(0.1, TimeSpan.FromSeconds(0), f));
            Anim.KeyFrames.Add(new EasingDoubleKeyFrame(0.1, TimeSpan.FromSeconds(.2), f));
            Anim.KeyFrames.Add(new EasingDoubleKeyFrame(1, TimeSpan.FromSeconds(.5), f));
            Storyboard.SetTarget(Anim, b_hintergrund);
            Storyboard.SetTargetProperty(Anim, new PropertyPath("RenderTransform.ScaleY"));
            start.Children.Add(Anim);
            var da = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(.3));
            Storyboard.SetTarget(da, b_hintergrund);
            Storyboard.SetTargetProperty(da, new PropertyPath("RenderTransform.ScaleX"));
            start.Children.Add(da);
            start.Completed += start_Completed;
            this.Visibility = Visibility.Visible;
            start.Begin();
        }

        void start_Completed(object sender, EventArgs e)
        {
            if (!w_player.IsMouseOver && !Prop.DisableAnimations)
                GetEinklappen().Begin();
        }

        private void Minitimer_Tick(object sender, EventArgs e)
        {
            ((DispatcherTimer)sender).Stop();
            if (Prop.DisableAnimations)
                return;
            GetEinklappen().Begin();
        }

        public Storyboard GetEinklappen()
        {
            double t = 0.3;
            Einklappen = new Storyboard();
            Einklappen.Completed += Einklappen_Completed;
            //Einklappen.BeginTime = TimeSpan.FromSeconds(2);
            DoubleAnimationUsingKeyFrames dak = new DoubleAnimationUsingKeyFrames();
            dak.KeyFrames.Add(new EasingDoubleKeyFrame(g_info.ActualHeight, TimeSpan.FromSeconds(t), new CubicEase(){EasingMode = EasingMode.EaseOut}));
            Storyboard.SetTarget(dak, g_controls as DependencyObject);
            Storyboard.SetTargetProperty(dak, new PropertyPath(Grid.HeightProperty));
            Einklappen.Children.Add(dak);
            dak = new DoubleAnimationUsingKeyFrames();
            dak.KeyFrames.Add(new EasingDoubleKeyFrame(g_info.ActualWidth, TimeSpan.FromSeconds(t), new CubicEase(){EasingMode = EasingMode.EaseOut}));
            Storyboard.SetTarget(dak, g_controls as DependencyObject);
            Storyboard.SetTargetProperty(dak, new PropertyPath(Grid.WidthProperty));
            Einklappen.Children.Add(dak);
            DoubleAnimation da = new DoubleAnimation(0, TimeSpan.FromSeconds(t));
            Storyboard.SetTarget(da, g_controls as DependencyObject);
            Storyboard.SetTargetProperty(da, new PropertyPath(Grid.OpacityProperty));
            Einklappen.Children.Add(da);
            da = new DoubleAnimation(1, TimeSpan.FromSeconds(t));
            Storyboard.SetTarget(da, g_info as DependencyObject);
            Storyboard.SetTargetProperty(da, new PropertyPath(Grid.OpacityProperty));
            Einklappen.Children.Add(da);
            return Einklappen;
        }

        void Einklappen_Completed(object sender, EventArgs e)
        {
            g_controls.Visibility = Visibility.Collapsed;
            playerstate = State.Collapsed;
        }

        public Storyboard GetAusklappen()
        {
            Console.WriteLine("Neues Ausklappen");
            double t = 0.3;
            Ausklappen = new Storyboard();
            //Ausklappen.Duration = TimeSpan.FromSeconds(t);
            Ausklappen.Completed += Ausklappen_Completed;
            DoubleAnimationUsingKeyFrames dak = new DoubleAnimationUsingKeyFrames();
            dak.KeyFrames.Add(new EasingDoubleKeyFrame(360, TimeSpan.FromSeconds(t), new CubicEase() { EasingMode = EasingMode.EaseOut }));
            Storyboard.SetTarget(dak, g_controls as DependencyObject);
            Storyboard.SetTargetProperty(dak, new PropertyPath(Grid.HeightProperty));
            Ausklappen.Children.Add(dak);
            dak = new DoubleAnimationUsingKeyFrames();
            dak.KeyFrames.Add(new EasingDoubleKeyFrame(600, TimeSpan.FromSeconds(t), new CubicEase() { EasingMode = EasingMode.EaseOut }));
            Storyboard.SetTarget(dak, g_controls as DependencyObject);
            Storyboard.SetTargetProperty(dak, new PropertyPath(Grid.WidthProperty));
            Ausklappen.Children.Add(dak);
            var da = new DoubleAnimation(1, TimeSpan.FromSeconds(t));
            Storyboard.SetTarget(da, g_controls as DependencyObject);
            Storyboard.SetTargetProperty(da, new PropertyPath(Grid.OpacityProperty));
            Ausklappen.Children.Add(da);
            da = new DoubleAnimation(0, TimeSpan.FromSeconds(t));
            Storyboard.SetTarget(da, g_info as DependencyObject);
            Storyboard.SetTargetProperty(da, new PropertyPath(Grid.OpacityProperty));
            Ausklappen.Children.Add(da);
            g_controls.Visibility = Visibility.Visible;
            return Ausklappen;
        }

        void Ausklappen_Completed(object sender, EventArgs e)
        {
            Prop.Track = Prop.Track;
            playerstate = State.Expanded;
        }

        public string Titel
        {
            set
            {
                Dispatcher.Invoke(() =>
                    {
                        l_titel.Content = value;
                        l_titel1.Content = value;
                        l_titel1.ToolTip = value;
                    });
            }
        }

        public string Interpret
        {
            set
            {
                Dispatcher.Invoke(() =>
                {
                    l_performer.Content = value;
                    l_performer1.Content = value;                    
                });
            }
        }

        public string Album
        {
            set
            {
                Dispatcher.Invoke(() =>
                {
                    l_album.Content = value;
                    l_album1.Content = value;
                });
            }
        }

        public BitmapImage Cover
        {
            set
            {
                Dispatcher.Invoke(() =>
                {
                    if (value != null)
                    {
                        i_cover.Source = value;
                        i_minicover.Source = value;
                    }
                    else
                    {
                        
                        i_cover.Source = Logic.Icon;
                        i_minicover.Source = Logic.Icon;                        
                    }
                    DoubleAnimation da = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(1));
                    i_cover.BeginAnimation(Image.OpacityProperty, da);
                    i_minicover.BeginAnimation(Image.OpacityProperty, da);
                });
            }
        }

        public TimeSpan Position
        {
            set
            {
                l_position.Content = value.ToString(@"hh\:mm\:ss");
                il_positionmini.Text = value.ToString(@"hh\:mm\:ss");
                if (!(s_position.IsMouseOver && Mouse.LeftButton.Equals(MouseButtonState.Pressed)))
                    s_position.Value = value.TotalMilliseconds;
                pb_position.Value = value.TotalMilliseconds;
            }
        }

        public TimeSpan Länge
        {
            set
            {
                Dispatcher.Invoke(() =>
                {
                    l_length.Content = value.ToString(@"hh\:mm\:ss");
                    il_lengthmini.Text = value.ToString(@"hh\:mm\:ss");
                    pb_position.Maximum = value.TotalMilliseconds;
                    s_position.Maximum = value.TotalMilliseconds;
                    s_position.TickFrequency = value.TotalMilliseconds / 100;
                }, System.Windows.Threading.DispatcherPriority.Background);
            }
        }

        public string Playlist
        {
            set
            {
                Dispatcher.Invoke(() =>
                {
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        r_playlist.Text = null;
                        r_playlist1.Text = null;
                        return;
                    }
                    r_playlist.Text = string.Format("[{0}]", value);
                    r_playlist1.Text = string.Format("[{0}]", value);
                });
            }
        }

        public int Track
        {
            set
            {
                Dispatcher.Invoke(() =>
                {
                    r_track.Text = (value+1).ToString();
                    r_track1.Text = (value+1).ToString();
                });
            }
        }

        public int Tracks
        {
            set
            {
                Dispatcher.Invoke(() =>
                {
                   r_tracks.Text = Prop.Playlist.Count.ToString();
                   r_tracks1.Text = Prop.Playlist.Count.ToString();
                });
            }
        }

        public bool Busy
        {
            set
            {
                Dispatcher.Invoke(() =>
                    {
                        if (value)
                            pb.Visibility = Visibility.Visible;
                        else
                            pb.Visibility = Visibility.Collapsed;
                    });
            }
        }

        public string Status
        {
            set
            {
                Dispatcher.Invoke(() =>
                {
                    l_status.Content = value;
                });
            }
        }

        private void w_player_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void w_player_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Logic.Beenden();
        }

        private void s_position_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if ((s_position.IsMouseOver && Mouse.LeftButton.Equals(MouseButtonState.Pressed)))
                Logic.SetPosition(TimeSpan.FromMilliseconds(e.NewValue));
        }

        private void s_balance_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            s_balance.Value = 0;
        }

        private void s_balance_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Logic.SetPan(s_balance.Value);
        }

        private void b_zurück_Click(object sender, RoutedEventArgs e)
        {
            Logic.Previous();
        }

        private void b_vor_Click(object sender, RoutedEventArgs e)
        {
            Logic.Next();
        }

        private void b_laden_Click(object sender, RoutedEventArgs e)
        {
            Logic.AddTracks();
        }

        private void b_leeren_Click(object sender, RoutedEventArgs e)
        {
            Logic.Clear();
        }

        private void b_play_Click(object sender, RoutedEventArgs e)
        {
            if (Prop.Playing)
                Logic.Pause();
            else
                Logic.Play();
        }

        private void s_speed_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Prop.Speed = 1;
        }

        private void cb_Wiederholen_Click(object sender, RoutedEventArgs e)
        {
            Prop.Repeat = (sender as CheckBox).IsChecked.Value;
        }

        private void cb_zufall_Click(object sender, RoutedEventArgs e)
        {
            Prop.Random = (sender as CheckBox).IsChecked.Value;
        }

        private void cb_mute_Click(object sender, RoutedEventArgs e)
        {
            Prop.Muting = (sender as CheckBox).IsChecked.Value;
        }

        private void cb_positonmerken_Click(object sender, RoutedEventArgs e)
        {
            Prop.RememberPosition = (sender as CheckBox).IsChecked.Value;
        }

        private void b_schließen_Click(object sender, RoutedEventArgs e)
        {
            Storyboard stop = new Storyboard();
            DoubleAnimation da = new DoubleAnimation(0, TimeSpan.FromSeconds(.3));
            b_hintergrund.RenderTransform = new ScaleTransform();
            Storyboard.SetTarget(da, b_hintergrund);
            Storyboard.SetTargetProperty(da, new PropertyPath("RenderTransform.ScaleX"));
            stop.Children.Add(da);
            da = new DoubleAnimation(0, TimeSpan.FromSeconds(.3));
            Storyboard.SetTarget(da, b_hintergrund);
            Storyboard.SetTargetProperty(da, new PropertyPath("RenderTransform.ScaleY"));
            stop.Children.Add(da);
            //da = new DoubleAnimation(0, TimeSpan.FromSeconds(.5));
            //Storyboard.SetTarget(da, PropertySet.MediaPlayer);
            //Storyboard.SetTargetProperty(da, MediaPlayer.vo);
            //stop.Children.Add(da);
            stop.Completed += stop_Completed;
            stop.Begin();
        }

        void stop_Completed(object sender, EventArgs e)
        {
            Close();
        }

        private void s_lautstärke_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Prop.Volume = 0.8;
        }

        private void lb_tracks_MouseLeave(object sender, MouseEventArgs e)
        {

        }

        private void tb_suche_GotFocus(object sender, RoutedEventArgs e)
        {
            l_suchen.Visibility = Visibility.Collapsed;
        }

        private void tb_suche_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tb_suche.Text))
            {
                l_suchen.Visibility = Visibility.Visible;
            }
        }

        private void tb_suche_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!w_player.IsLoaded)
                return;

            if (string.IsNullOrWhiteSpace(tb_suche.Text))
                b_suchelöschen.Visibility = Visibility.Collapsed;                
            else
                b_suchelöschen.Visibility = Visibility.Visible;

                Logic.SearchTracks(tb_suche.Text);
        }

        private void b_suchelöschen_Click(object sender, RoutedEventArgs e)
        {
            l_suchen.Visibility = Visibility.Visible;
            b_suchelöschen.Visibility = Visibility.Collapsed;
            lb_tracks.Focus();
        }

        private void cb_server_Click(object sender, RoutedEventArgs e)
        {
            //Logic.ServerChange(cb_server.IsChecked.Value);
        }

        private void lb_tracks_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            bool b = lb_tracks.SelectedItem != null;
            mi_entfernen.IsEnabled = b;
            mi_oben.IsEnabled =b;
            mi_unten.IsEnabled = b;
            mi_tags.IsEnabled = b && Prop.Track != lb_tracks.SelectedIndex;

            mi_vonvorn.IsEnabled = Prop.Playlist.Count > 0;
        }

        private void lb_tracks_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lb_tracks.SelectedItem != null)
            {
                Logic.Play(lb_tracks.SelectedIndex);
                TracksEinklappen();
            }
        }

        private void s_speed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if ((s_speed.IsMouseOver && Mouse.LeftButton.Equals(MouseButtonState.Pressed)))
                Prop.Speed = s_speed.Value;
        }

        private void s_lautstärke_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Prop.Volume = s_lautstärke.Value / 100;
        }

        private void mi_vonvorn_Click(object sender, RoutedEventArgs e)
        {
            Logic.VonVorn();
        }

        private void mi_neu_Click(object sender, RoutedEventArgs e)
        {
            Logic.NeuePlaylist();
        }

        private void mi_umbenennen_Click(object sender, RoutedEventArgs e)
        {
            if (lb_playlists.SelectedItem != null)
                Logic.PlaylistUmbenennen((Playlist)lb_playlists.SelectedItem);
        }

        private void mi_hinzufügen_Click(object sender, RoutedEventArgs e)
        {
            if (lb_playlists.SelectedItem != null)
                Logic.WiedergabeInPlaylist((Playlist)lb_playlists.SelectedItem);
        }

        private void mi_track_Click(object sender, RoutedEventArgs e)
        {
            if (lb_playlists.SelectedItem != null)
                Logic.TrackInPlaylist(lb_playlists.SelectedItem as Playlist);
        }

        private void mi_tauschen_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mi_entfernen_Click(object sender, RoutedEventArgs e)
        {
            if (lb_tracks.SelectedItem != null)
            {
                Logic.RemoveTrack((Track) lb_tracks.SelectedItem);
            }
        }        

        private void mi_playlists_Click(object sender, RoutedEventArgs e)
        {
            SlideToPlaylists();
        }

        public void SlideToPlaylists()
        {
            if (lb_tracks.Items.Count > 0)
                lb_tracks.ScrollIntoView(lb_tracks.Items[0]);
            lb_playlists.Visibility = Visibility.Visible;
            g_suche.Visibility = Visibility.Collapsed;
            Storyboard sb = new Storyboard();
            DoubleAnimationUsingKeyFrames da = new DoubleAnimationUsingKeyFrames();
            da.KeyFrames.Add(new EasingDoubleKeyFrame(sv_tracks.ActualWidth, TimeSpan.FromSeconds(1), new CubicEase()));
            lb_tracks.RenderTransform = new TranslateTransform();
            Storyboard.SetTarget(da, lb_tracks);
            Storyboard.SetTargetProperty(da, new PropertyPath("RenderTransform.X"));
            sb.Children.Add(da);

            da = new DoubleAnimationUsingKeyFrames();
            da.KeyFrames.Add(new EasingDoubleKeyFrame(-(lb_tracks.ActualWidth), TimeSpan.FromSeconds(0), new CubicEase()));
            da.KeyFrames.Add(new EasingDoubleKeyFrame(0, TimeSpan.FromSeconds(1), new CubicEase()));
            lb_playlists.RenderTransform = new TranslateTransform();
            Storyboard.SetTarget(da, lb_playlists);
            Storyboard.SetTargetProperty(da, new PropertyPath("RenderTransform.X"));
            sb.Children.Add(da);
            sb.Completed += (o, i) =>
            {
                lb_tracks.Visibility = Visibility.Collapsed;
            };
            sb.Begin();
        }

        private void mi_tracks_Click(object sender, RoutedEventArgs e)
        {
            SlideToTracks();    
        }

        public void SlideToTracks()
        {
            lb_tracks.Visibility = Visibility.Visible;
            Storyboard sb = new Storyboard();
            DoubleAnimationUsingKeyFrames da = new DoubleAnimationUsingKeyFrames();
            da.KeyFrames.Add(new EasingDoubleKeyFrame((lb_tracks.ActualWidth), TimeSpan.FromSeconds(0), new CubicEase()));
            da.KeyFrames.Add(new EasingDoubleKeyFrame(0, TimeSpan.FromSeconds(1), new CubicEase()));
            lb_tracks.RenderTransform = new TranslateTransform();
            Storyboard.SetTarget(da, lb_tracks);
            Storyboard.SetTargetProperty(da, new PropertyPath("RenderTransform.X"));
            sb.Children.Add(da);

            da = new DoubleAnimationUsingKeyFrames();
            da.KeyFrames.Add(new EasingDoubleKeyFrame(-(lb_playlists.ActualWidth), TimeSpan.FromSeconds(1), new CubicEase()));
            lb_playlists.RenderTransform = new TranslateTransform();
            Storyboard.SetTarget(da, lb_playlists);
            Storyboard.SetTargetProperty(da, new PropertyPath("RenderTransform.X"));
            sb.Children.Add(da);
            sb.Completed += (o, i) =>
            {
                lb_playlists.Visibility = Visibility.Collapsed;
                g_suche.Visibility = Visibility.Visible;
            };
            sb.Begin();
        }

        private void mi_oben_Click(object sender, RoutedEventArgs e)
        {
            if (lb_tracks.SelectedItem != null)
            {
                Logic.NachOben(lb_tracks.SelectedItem as Track);
            }
        }

        private void mi_unten_Click(object sender, RoutedEventArgs e)
        {
            if (lb_tracks.SelectedItem != null)
            {
                Logic.NachUnten(lb_tracks.SelectedItem as Track);
            }
        }

        private void mi_laden_Click(object sender, RoutedEventArgs e)
        {

        }

        private void lb_tracks_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void sv_tracks_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sv_tracks.Height == 60 || lb_tracks.ContextMenu.IsOpen || lb_playlists.ContextMenu.IsOpen)
                return;
            TracksEinklappen();
        }

        private void sv_tracks_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sv_tracks.Height == 300)
                return;
            TracksAusklappen();
        }

        public void TracksEinklappen()
        {
            DoubleAnimation da = new DoubleAnimation(60, TimeSpan.FromSeconds(0.2));
            da.Completed += (o, i) =>
            {
                Prop.Track = Prop.Track;
            };
            sv_tracks.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
            sv_tracks.BeginAnimation(StackPanel.HeightProperty, da);
        }

        public void TracksAusklappen()
        {
            DoubleAnimation da = new DoubleAnimation(300, TimeSpan.FromSeconds(0.2));
            da.Completed += (o, i) =>
            {
                sv_tracks.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            };
            sv_tracks.BeginAnimation(StackPanel.HeightProperty, da);
        }

        private void lb_playlists_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lb_playlists.SelectedItem != null)
            {
                Prop.View.Busy = true;
                lb_playlists.IsEnabled = false;
                Playlist p = lb_playlists.SelectedItem as Playlist;
                new Thread(() =>
                    {
                        if (!string.IsNullOrEmpty(Prop.Playlistpath))
                        {
                            Player.Playlist.SaveToPlaylist(Prop.Playlist, Prop.Playlistpath);
                        }
                        Logic.LoadPlaylist(p.Path);
                        Prop.Track = 0;
                        if (Prop.Playlist.Count > 0)
                        {
                            Dispatcher.Invoke(() => g_controls.IsEnabled = false);
                            Dispatcher.Invoke(() => Prop.MediaPlayer.Open(new Uri(Prop.Playlist[Prop.Track].Path)));
                            Dispatcher.Invoke(() => Prop.MediaPlayer.Volume = Prop.Volume);
                            Dispatcher.Invoke(() => Prop.MediaPlayer.Play());
                            Dispatcher.Invoke(() => g_controls.IsEnabled = true);
                            Prop.Playing = true;
                        }
                        else
                            Prop.Playing = false;
                       Prop.View.Busy = false;
                    }).Start();
                SlideToTracks();
                lb_playlists.IsEnabled = true;
            }
        }

        private void mi_playlistentfernen_Click(object sender, RoutedEventArgs e)
        {
            if (lb_playlists.SelectedItem != null)
                Logic.PlaylistEntfernen((Playlist)lb_playlists.SelectedItem);
        }

        private void lb_playlists_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            bool b = lb_playlists.SelectedItem != null;
            mi_playlistentfernen.IsEnabled = b;

        }

        private void lb_tracks_MouseMove(object sender, MouseEventArgs e)
        {
            //if (Mouse.LeftButton.Equals(MouseButtonState.Pressed) && lb_tracks.SelectedItem != null)
            //    DragDrop.DoDragDrop(lb_tracks, lb_tracks.SelectedItem, DragDropEffects.Move);
        }

        private void lb_tracks_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void mi_background_Click(object sender, RoutedEventArgs e)
        {
            Logic.OpenImage();
        }

        private void mi_backgroundentfernen_Click(object sender, RoutedEventArgs e)
        {
            Prop.BackgroundImage = null;
        }

        private void w_player_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (Prop.BackgroundImage != null)
            {
                mi_background.Visibility = System.Windows.Visibility.Collapsed;
                mi_backgroundentfernen.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                mi_background.Visibility = System.Windows.Visibility.Visible;
                mi_backgroundentfernen.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void cb_animationen_Click(object sender, RoutedEventArgs e)
        {
            Prop.DisableAnimations = cb_animationen.IsChecked.Value;
            if (Prop.DisableAnimations)
            {
                if (Einklappen != null)
                    Einklappen.Stop();
                GetAusklappen().Begin();
            }
            else
            {
                if (Ausklappen != null)
                    Ausklappen.Stop();
                GetEinklappen().Begin();
            }
        }

        private void w_player_LocationChanged(object sender, EventArgs e)
        {
            Prop.WindowPosition = new Point(Left, Top);
        }

        private void mi_tags_Click(object sender, RoutedEventArgs e)
        {
            Logic.EditTrack(lb_tracks.SelectedItem as Track);
        }

        private void w_player_PreviewMouseMove(object sender, MouseEventArgs e)
        {     
            if (playerstate.Equals(State.Collapsing) || playerstate.Equals(State.Expanding))
            {
                return;
            }
            if (playerstate.Equals(State.Collapsed))
            {
                GetAusklappen().Begin();
            }
            minitimer.Stop();
            minitimer.Start();      
        }
    }
}
