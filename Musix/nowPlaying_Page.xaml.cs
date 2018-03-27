using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using WPFSoundVisualizationLib;
using Finisar.SQLite;
using System.Windows.Data;
using System.ComponentModel;

namespace Musix
{    
    public partial class nowPlaying_Page : Page
    {
        public nowPlaying_Page(bool IsNewSong)
        {
            InitializeComponent();
            isNewSong = IsNewSong;
        }        

        private void Playlist_MediaChange(object sender, PlaylistEventArgs e)
        {
            init(e.Path);                       
        }

        int startIndex;
        bool isNewSong;        
        TagLib.File song;
        DispatcherTimer timer = new DispatcherTimer();
        TimeSpan timeElapsed;

        private void timer_Tick(object sender, EventArgs e)
        {
            if ((App.Current as App).player.Source != null)
            {
                timeElapsed = (App.Current as App).player.Position;
                timeElapsed_Label.Content = timeElapsed.ToString(@"mm\:ss");
                timeElapsed_progressBar.Value = timeElapsed.TotalSeconds;
                if (song.Properties.Duration == timeElapsed)
                    (App.Current as App).nowPlaying_Status = 0;
            }
        }
        
        private void back_Button_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void volume_Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            (App.Current as App).player.Volume = volume_Slider.Value;            
        }

        private void playPause_Click(object sender, RoutedEventArgs e)
        {

            if ((App.Current as App).nowPlaying_Status == 1)
            {
                (App.Current as App).player.Pause();
                timer.IsEnabled = false;                
                (App.Current as App).nowPlaying_Status = 2;
                playPause_ButtonImage.ImageSource = new BitmapImage(new Uri("PLay_48.png",UriKind.Relative));
            }
            else if ((App.Current as App).nowPlaying_Status == 2)
            {
                (App.Current as App).player.Play();
                timer.IsEnabled = true;
                (App.Current as App).nowPlaying_Status = 1;
                playPause_ButtonImage.ImageSource = new BitmapImage(new Uri("Pause_48.png", UriKind.Relative));
            }           
        }
        private void timeElapsed_progressBar_MouseUp(object sender, MouseButtonEventArgs e)
        {            
            Point p = e.GetPosition(timeElapsed_progressBar);
            double percent = p.X / timeElapsed_progressBar.ActualWidth;
            timeElapsed_progressBar.Value = timeElapsed_progressBar.Maximum * percent;
            (App.Current as App).player.Pause();
            (App.Current as App).player.Position = new TimeSpan(0, 0, (int)(song.Properties.Duration.TotalSeconds * percent));
            (App.Current as App).player.Play();
        }
        private void timeElapsed_progressBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            System.Windows.MessageBox.Show("clicked");
            if ((App.Current as App).nowPlaying_Status == 1)
            {                
                Point p = e.GetPosition(timeElapsed_progressBar);
                double percent = p.X / timeElapsed_progressBar.ActualWidth;
                TimeSpan t = new TimeSpan(0, 0, (int)(song.Properties.Duration.TotalSeconds * percent));                
                timeElapsed_Label.Content = t.ToString();
            }
        }
        private void PageLoaded(object sender, RoutedEventArgs e)
        {
            if (isNewSong)
            {
                (App.Current as App).playlist = null;
                (App.Current as App).playlist = new Playlist((App.Current as App).player, (App.Current as App).paths);
                (App.Current as App).playlist.Play((App.Current as App).startIndex);                
            }
            loadSongs();
            (App.Current as App).nowPlaying_Status = 1;
            (App.Current as App).playlist.MediaChange += Playlist_MediaChange;
            init((App.Current as App).paths[(App.Current as App).playlist.PlayIndex]);
        }
        private void init(string path)
        {            
            song_list.SelectedIndex = (App.Current as App).playlist.PlayIndex;
            song = TagLib.File.Create(path);
            nowPlaying_Title.Content = song.Tag.Title;
            nowPlaying_Artist.Content = song.Tag.FirstAlbumArtist + " | " + song.Tag.Album;
            timeSpan_Label.Content = song.Properties.Duration.ToString(@"mm\:ss");
            //progress bar setup
            timeElapsed_progressBar.Maximum = (int)song.Properties.Duration.TotalSeconds;
            timeElapsed_progressBar.Minimum = timeElapsed_progressBar.Value = 0;
            //starting timer            
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;
            timer.Start();
            try
            {
                nowPlaying_AlbumArt.Source = Background_image.Source = Mp3Info.GetAlbumArt(path);
            }
            catch { }
            (App.Current as App).player.Volume = volume_Slider.Value;

            //getting lyrics
            try
            {
                //AzLyrics lyrics = new AzLyrics(song.Tag.FirstAlbumArtist, song.Tag.Title);                
                //Lyrics_textBlock.Text = lyrics.GetLyris();
                LyricsWikia lyrics = new LyricsWikia(song.Tag.FirstAlbumArtist, song.Tag.Title);
                Lyrics_textBlock.Text = lyrics.GetLyris();
            }
            catch(System.Net.WebException)
            {
                Lyrics_textBlock.Text = "Lyrics not found!";
            }
            catch(Exception ex)
            {
                Lyrics_textBlock.Text = ex.ToString();
            }
        }
        private void loadSongs()
        {
            List<Songs> list = new List<Songs>();
            foreach (var item in (App.Current as App).paths)
            {
                Mp3Info info = new Mp3Info(item);
                string duration = new TimeSpan(0, 0,info.Duration).ToString(@"mm\:ss");
                list.Add(new Songs() { Title = info.Title, Duration = duration });
            }
            //if (song_list.ItemsSource != null)
            //    song_list.ItemsSource = null;
            song_list.Items.Clear();
            song_list.ItemsSource = list;
            song_list.SelectedIndex = (App.Current as App).playlist.PlayIndex;            
        }
        class Songs
        {
            public string Title { get; set; }            
            public string Duration { get; set; }
        }

        private void next_Click(object sender, RoutedEventArgs e)
        {
            (App.Current as App).playlist.Next();
        }

        private void song_list_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            (App.Current as App).playlist.Play(song_list.SelectedIndex);
            init((App.Current as App).paths[(App.Current as App).playlist.PlayIndex]);
        }

        private void shuffle_Button_Click(object sender, RoutedEventArgs e)
        {
            (App.Current as App).playlist.Shuffle();
            song_list.ItemsSource = null;            
            loadSongs();
        }
    }
}