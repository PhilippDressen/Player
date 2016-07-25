using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;

namespace Player
{
    public class PlayHub : Hub
    {
        public PlayHub()
        {
            Prop.Hub = this;
        }

        public override Task OnConnected()
        {
            Log.Write("Remoteclient connected!", EventType.Info);
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            Log.Write("Remoteclient disconnected! (called: " + stopCalled + ")", EventType.Info);
            return base.OnDisconnected(stopCalled);
        }

        public async Task<bool> PlayPause()
        {
            if (Prop.Playing)
            {
                App.Current.Dispatcher.Invoke(Prop.MediaPlayer.Pause);
                Prop.Playing = false;
            }
            else
            {
                App.Current.Dispatcher.Invoke(Prop.MediaPlayer.Play);
                Prop.Playing = true;
            }
            return Prop.Playing;
        }

        public async Task Play(int i)
        {
            if (i < Prop.Playlist.Count)
            {
                Prop.Track = i;
                App.Current.Dispatcher.Invoke(() => Prop.View.g_controls.IsEnabled = false);
                App.Current.Dispatcher.Invoke(() => Prop.MediaPlayer.Open(new Uri(Prop.Playlist[Prop.Track].Path)));
                if (Prop.Playing && Prop.Playlist.Count > 0)
                {
                    App.Current.Dispatcher.Invoke(() => Prop.MediaPlayer.Play());
                }
                else
                    Prop.Playing = false;
                App.Current.Dispatcher.Invoke(() => Prop.View.g_controls.IsEnabled = true);                
            }
        }

        public async Task Next()
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

        public async Task Previous()
        {
            if (App.Current.Dispatcher.Invoke<TimeSpan>(() => Prop.MediaPlayer.Position) > TimeSpan.FromSeconds(3))
            {
                App.Current.Dispatcher.Invoke(() => Prop.MediaPlayer.Position = new TimeSpan());
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

        public async Task<bool> GetPlaying()
        {
            return Prop.Playing;
        }

        public async Task<TimeSpan> GetPosition()
        {
            return App.Current.Dispatcher.Invoke<TimeSpan>(()=> Prop.MediaPlayer.Position);
        }

        public async Task SetPosition(TimeSpan s)
        {
            App.Current.Dispatcher.Invoke(() => Prop.MediaPlayer.Position = s);
        }

        public async Task SetVolume(double d)
        {
            Prop.Volume = d;
        }

        public async Task<TimeSpan> GetLength()
        {
            TimeSpan ts = new TimeSpan();
            App.Current.Dispatcher.Invoke(() =>
                {
                    if (Prop.MediaPlayer.NaturalDuration.HasTimeSpan)
                        ts = Prop.MediaPlayer.NaturalDuration.TimeSpan;
                });
            return ts;
        }

        public async Task<double> GetVolume()
        {
            return App.Current.Dispatcher.Invoke<double>(() => Prop.MediaPlayer.Volume);
        }

        public async Task<int> GetTrack()
        {
            return Prop.Track;
        }

        public async Task<Track> GetTrack(int i)
        {
            Track t = Prop.Playlist[i];
            t.AlbumArt = null;
            return t;
        }

        public async Task<int> GetTrackCount()
        {
            return Prop.Playlist.Count;
        }

        public async Task<List<string>> GetPlaylist()
        {
            List<string> list = new List<string>();
            foreach (Track t in Prop.Playlist)
            {
                string ti = t.Title;
                foreach (char c in System.IO.Path.GetInvalidFileNameChars())
                {
                    if (ti.Contains(c))
                    {
                        Console.WriteLine(t.Title + " " + c);
                        ti = ti.Replace(c.ToString(), "");
                    }
                       
                }
                list.Add(ti);
                
            }
            return list;
        }

        public async Task Stop()
        {
            App.Current.Dispatcher.Invoke(Prop.MediaPlayer.Stop);
            Prop.Playing = false;
            Clients.All.Playing(Prop.Playing);
        }
    }
}
