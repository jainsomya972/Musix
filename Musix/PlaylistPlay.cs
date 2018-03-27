using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Threading.Tasks;

namespace Musix
{
    public class PlaylistEventArgs:System.EventArgs
    {
        public Mp3Info info;
        public string Path;
        public PlaylistEventArgs(string path)
        {
            info = new Mp3Info(path);
            Path = path;
        }
    }
    public delegate void PlaylistEventHandler(object sender, PlaylistEventArgs e);
    public class Playlist
    {
        public event PlaylistEventHandler MediaChange;
        MediaPlayer player;
        List<string> songPaths;
        int currentPlayIndex;        
        public int PlayIndex { get { return currentPlayIndex; } }
        int length;
        public Playlist(MediaPlayer Player,List<string> SongPaths)
        {
            player = Player;
            songPaths = SongPaths;
            currentPlayIndex = 0;
            length = songPaths.Count;
            player.MediaEnded += Player_MediaEnded;
        }

        private void Player_MediaEnded(object sender, EventArgs e)
        {
            if(++currentPlayIndex<length)
            {
                player.Close();
                player.Open(new Uri(songPaths[currentPlayIndex]));
                player.Play();
                OnMediaChange(new PlaylistEventArgs(songPaths[currentPlayIndex]));
            }           
        }

        public void Play(int index)
        {
            currentPlayIndex = index;
            if(player.HasAudio)
            {
                player.Close();
            }
            player.Open(new Uri(songPaths[currentPlayIndex]));
            player.Play();
            //OnMediaChange(new PlaylistEventArgs(songPaths[currentPlayIndex]));
        }
        public void Play()
        {
            if (player.HasAudio)
            {
                player.Close();
            }
            player.Open(new Uri(songPaths[currentPlayIndex]));
            player.Play();
        }
        public void AddSong(string path)
        {
            length++;
            songPaths.Add(path);
        }
        public void Next()
        {
            if (++currentPlayIndex < length)
            { 
            player.Open(new Uri(songPaths[currentPlayIndex]));
            player.Play();
            OnMediaChange(new PlaylistEventArgs(songPaths[currentPlayIndex]));
            }
        }
        protected virtual void OnMediaChange(PlaylistEventArgs e)
        {
            if (MediaChange != null) MediaChange(this, e);
        }
        public void Shuffle()
        {
            Random rnd = new Random();
            int n = length;
            string x;
            while(n>1)
            {
                int k = rnd.Next(0, n);
                n--;
                if (k == currentPlayIndex)
                    currentPlayIndex = n;
                x = songPaths[k];
                songPaths[k] = songPaths[n];
                songPaths[n] = x;                
            }
            //taking now plaing song to top
            x = songPaths[0];
            songPaths[0] = songPaths[currentPlayIndex];
            songPaths[currentPlayIndex] = x;
            currentPlayIndex = 0;
        }
    }
}
