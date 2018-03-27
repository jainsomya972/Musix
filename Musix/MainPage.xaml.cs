using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.ComponentModel;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Windows.Threading;
using Finisar.SQLite;
using Microsoft.Win32;

namespace Musix
{
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {
        DatabaseBuilder d;
       

        public MainPage()
        {
            InitializeComponent();
            string path = File.ReadAllText("folders.mus");
            d = new DatabaseBuilder(path);
        }
        
        private void Page_load(object sender, RoutedEventArgs e)
        {           
            if(App.Current.Properties["startupFiles"]!=null)
            {
                nowPlaying_Page page = new nowPlaying_Page(true);
                NavigationService.Navigate(page);
                App.Current.Properties["startupFiles"] = null;
            }
            else if (!File.Exists("database.db"))
            {
                Task t = Task.Factory.StartNew(() => d.createDatabase())
                    .ContinueWith(ignore => loadArtists())
                    .ContinueWith(ignore => loadSongs())
                    .ContinueWith(ignore => loadGenre())
                    .ContinueWith(ignore => loadAlbums());
            }
            else
            {
                loadArtists();
                loadSongs();
                loadGenre();
                loadAlbums();
            }
            
        }

        BitmapImage pauseImage = new BitmapImage(new Uri("Pause_48.png",UriKind.Relative));
        BitmapImage playImage = new BitmapImage(new Uri("Play_48.png",UriKind.Relative));
        BitmapImage stopImage = new BitmapImage(new Uri("Stop_48.png",UriKind.Relative));        

        private void loadArtists()
        {
            List<string> loadedArtists = new List<string>();           
            loadedArtists = DatabaserSeeker.GetArtists("database.db","music");
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => artist_listView.ItemsSource = loadedArtists);
                return;
            }
            else
                artist_listView.ItemsSource = loadedArtists;
        }
        private void loadGenre()
        {
            List<string> loadedGenre = new List<string>();            
            loadedGenre = DatabaserSeeker.GetGenres("database.db", "music");
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => genre_list.ItemsSource = loadedGenre);
                return;
            }
            else
                genre_list.ItemsSource = loadedGenre;


        }
        private void loadSongs()
        {
            List<Songs> list = new List<Songs>();

            SQLiteConnection connection = new SQLiteConnection("Data Source=database.db;Version=3;New=False;Compress=True;");
            connection.Open();
            SQLiteCommand command = connection.CreateCommand();

            command.CommandText = "select Title, Artist, Album, duration from music";
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                string title = reader.GetString(0);
                if(title==null||title=="")
                {
                    title = "unknown title";
                }
                string duration= new TimeSpan(0, 0, reader.GetInt32(3)).ToString(@"mm\:ss");

                list.Add(new Songs() { Title= title, Artist= reader.GetString(1), Album = reader.GetString(2), Duration =  duration});
            }
            if(!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() =>
                {
                    song_list.ItemsSource = list;
                    CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(song_list.ItemsSource);
                    view.SortDescriptions.Add(new SortDescription("Title", ListSortDirection.Ascending));
                });
                return;
            }  
            else
            {
                song_list.ItemsSource = list;
                CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(song_list.ItemsSource);
                view.SortDescriptions.Add(new SortDescription("Title", ListSortDirection.Ascending));
            }
            connection.Close();            

        }
        class Songs
        {
            public string Title { get; set; }
            public string Artist { get; set; }
            public string Album { get; set; }
            public string Duration { get; set; }            
        }
        private void loadAlbums()
        {
            List<Albums> list = new List<Albums>();

            SQLiteConnection connection = new SQLiteConnection("Data Source=database.db;Version=3;New=False;Compress=True;");
            connection.Open();
            SQLiteCommand command = connection.CreateCommand();

            command.CommandText = "select Album, artist, year from music group by Album";
            SQLiteDataReader reader = command.ExecuteReader();
            
            while (reader.Read())
            {
                Albums album = new Albums();
                album.Album = reader.GetString(0);
                album.Artist = reader.GetString(1);
                try
                {
                    album.image = Mp3Info.GetAlbumArt(Mp3Info.GetFilePath(album.Album, 0));
                }
                catch
                {
                    BitmapImage i = new BitmapImage(new Uri("music-album.png", UriKind.Relative));
                }
                list.Add(album);
            }                   
            if(!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() =>
                {
                    album_list.ItemsSource = list;
                    //CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(album_list.ItemsSource);
                    //view.SortDescriptions.Add(new SortDescription("Album", ListSortDirection.Ascending));
                });
                return;
            }
            else
            {
                album_list.ItemsSource = list;
                //CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(album_list.ItemsSource);
                //view.SortDescriptions.Add(new SortDescription("Album", ListSortDirection.Ascending));
            }
           
            connection.Close();

        }
        class Albums
        {
            public BitmapImage image { get; set; }
            public string Album { get; set; }
            public string Artist { get; set; }          
        }

        private void songList_DoubleClick(object sender, MouseButtonEventArgs e)
        {

            /*var s = song_list.SelectedItem as Songs;
            string path = Mp3Info.GetFilePath(s.Title, s.Artist);           
            (App.Current as App).nowPlaying_path = path;
            nowPlaying_Page page = new nowPlaying_Page(true);
            NavigationService.Navigate(page);    
            */
            List<string> paths = new List<string>();
            foreach (var item in song_list.Items)
            {
                var s = item as Songs;
                paths.Add(Mp3Info.GetFilePath(s.Title, s.Artist));
            }
            (App.Current as App).paths = paths;
            (App.Current as App).startIndex = song_list.SelectedIndex;
            nowPlaying_Page page = new nowPlaying_Page(true);
            NavigationService.Navigate(page);
        }
        TagLib.File song;
        DispatcherTimer timer = new DispatcherTimer();
        TimeSpan timeElapsed;
        private void timer_Tick(object sender, EventArgs e)
        {
            if((App.Current as App).player.Source!=null)
            {
                timeElapsed = (App.Current as App).player.Position;
                timeElapsed_Label.Content = timeElapsed.ToString(@"mm\:ss");
                timeElapsed_progressBar.Value = timeElapsed.TotalSeconds;
                if (song.Properties.Duration == timeElapsed)
                    (App.Current as App).nowPlaying_Status = 0;
            }
        }

        private void playPause_Click(object sender, RoutedEventArgs e)
        {
            if((App.Current as App).nowPlaying_Status ==1)
            {
                (App.Current as App).player.Pause();
                timer.IsEnabled = false;
                //timer.Stop();
                (App.Current as App).nowPlaying_Status = 2;
                playPause_ButtonImage.ImageSource = playImage;                
            }
            else if((App.Current as App).nowPlaying_Status ==2)
            {
                (App.Current as App).player.Play();
                timer.IsEnabled = true;
                (App.Current as App).nowPlaying_Status = 1;
                playPause_ButtonImage.ImageSource = pauseImage;
            }
        }

        private void stop_Click(object sender, RoutedEventArgs e)
        {
            if ((App.Current as App).nowPlaying_Status != 0)
            {
                (App.Current as App).player.Stop();
                (App.Current as App).nowPlaying_Status = 0;                
            }
        }

        private void timeElapsed_progressBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point p= e.GetPosition(timeElapsed_progressBar);
            double percent = p.X / timeElapsed_progressBar.Width;
            timeElapsed_progressBar.Value = timeElapsed_progressBar.Maximum * percent;
            (App.Current as App).player.Pause();
            (App.Current as App).player.Position = new TimeSpan(0, 0, (int)(song.Properties.Duration.TotalSeconds * percent));
            (App.Current as App).player.Play();
        }

        private void timeElapsed_progressBar_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if ((App.Current as App).nowPlaying_Status == 1)
            {
                Point p = e.GetPosition(timeElapsed_progressBar);
                double percent = p.X / timeElapsed_progressBar.Width;
                TimeSpan t = new TimeSpan(0, 0, (int)(song.Properties.Duration.TotalSeconds * percent));
                timeElapsed_progressBar.ToolTip = t.ToString(@"mm\:ss");
            }
        }

        private void changeFolder_Button_click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog browser = new FolderBrowserDialog();
            DialogResult result = browser.ShowDialog();
            if(result==DialogResult.OK)
            {
                string path = browser.SelectedPath;
                File.WriteAllText("folders.mus", path);
                (App.Current as App).player.Close();
                (App.Current as App).initializer();
                DatabaseBuilder D = new DatabaseBuilder(path);
                Task t = Task.Factory.StartNew(() => D.createDatabase())
                   .ContinueWith(ignore => loadArtists())
                   .ContinueWith(ignore => loadAlbums())
                   .ContinueWith(ignore => loadGenre())
                   .ContinueWith(ignore => loadSongs())
                   .ContinueWith(ignore => { int a = 5; });
                //t.Wait();
            }           
        }

        private void search_Button_Click(object sender, RoutedEventArgs e)
        {
            //searchEntry_TextBox.Items.Clear();              
            searchEntry_TextBox.ItemsSource = DatabaserSeeker.TitleSearch("database.db", "music", searchEntry_TextBox.Text);
            searchEntry_TextBox.IsDropDownOpen = true;
        }

        private void searchEntry_TextBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (searchEntry_TextBox.SelectedIndex >= 0)
            {
                string title = searchEntry_TextBox.SelectedItem.ToString();                
                string path = Mp3Info.GetFilePath(title);
                //(App.Current as App).paths.Clear();
                (App.Current as App).paths = new List<string>();
                (App.Current as App).paths.Add(path);
                (App.Current as App).startIndex = 0;
                //(App.Current as App).nowPlaying_path = path;
                nowPlaying_Page page = new nowPlaying_Page(true);
                NavigationService.Navigate(page);                
                searchEntry_TextBox.Text = "";
            }
            else
            {
                //searchEntry_TextBox.Items.Clear();
                searchEntry_TextBox.IsDropDownOpen = false;
            }
        }

        private void artist_listView_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (artist_listView.SelectedIndex >= 0)
            {
                ArtistPage page = new ArtistPage(artist_listView.SelectedItem.ToString());                
                NavigationService.Navigate(page);
            }
        }

        private void refreshButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private void expand_Button_Click(object sender, RoutedEventArgs e)
        {
            nowPlaying_Page page = new nowPlaying_Page(false);
            NavigationService.Navigate(page);
        }
        private void album_list_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            var s = album_list.SelectedItem as Albums;
            AlbumPage page = new AlbumPage(s.Album,s.Artist);
            NavigationService.Navigate(page);
        }

        private void Artist_tab_GotFocus(object sender, RoutedEventArgs e)
        {
            
        }

        private void genre_listView_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (genre_list.SelectedIndex >= 0)
            {
                string s = genre_list.SelectedItem.ToString();
                GenrePage page = new GenrePage(s);
                NavigationService.Navigate(page);
            }
        }

        private void AlbumList_Play_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void AlbumList_Play_Click(object sender, MouseButtonEventArgs e)
        {
            var s = album_list.SelectedItem as Albums;
            string album = s.Album;
            List<string> paths = new List<string>();
            SQLiteConnection connection = new SQLiteConnection("Data Source=database.db;Version=3;New=False;Compress=True;");
            connection.Open();
            SQLiteCommand command = connection.CreateCommand();

            command.CommandText = "select Title, duration from music where album='" + album + "';";
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                string title = reader.GetString(0);
                paths.Add(Mp3Info.GetFilePath(title));
            }
            if ((App.Current as App).paths != null)
                (App.Current as App).paths.Clear();
            (App.Current as App).paths = paths;
            (App.Current as App).startIndex = 0;
            nowPlaying_Page page = new nowPlaying_Page(true);
            NavigationService.Navigate(page);
        }
    }
}
