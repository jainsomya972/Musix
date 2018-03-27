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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Finisar.SQLite;
using System.ComponentModel;
using System.IO;

namespace Musix
{
    /// <summary>
    /// Interaction logic for ArtistPage.xaml
    /// </summary>
    public partial class ArtistPage : Page
    {
        public ArtistPage(string artist)
        {
            InitializeComponent();
            recievedArtist = artist;
        }
        string recievedArtist;      

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {            
            Artist_label.Content = recievedArtist;
            loadSongs();
            loadAlbums();
        }

        private void loadSongs()
        {
            List<Songs> list = new List<Songs>();

            SQLiteConnection connection = new SQLiteConnection("Data Source=database.db;Version=3;New=False;Compress=True;");
            connection.Open();
            SQLiteCommand command = connection.CreateCommand();

            command.CommandText = "select Title, Album, duration from music where artist='"+recievedArtist+"';";
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                string title = reader.GetString(0);
                if (title == null || title == "")
                {
                    title = "unknown title";
                }
                string duration = new TimeSpan(0, 0, reader.GetInt32(2)).ToString(@"mm\:ss");

                list.Add(new Songs() { Title = title, Album = reader.GetString(1), Duration = duration });
            }
            song_list.ItemsSource = list;
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(song_list.ItemsSource);
            view.SortDescriptions.Add(new SortDescription("Title", ListSortDirection.Ascending));
            connection.Close();

        }
        class Songs
        {
            public string Title { get; set; }
            public string Album { get; set; }
            public string Duration { get; set; }
        }

        private void songList_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            /*var s = song_list.SelectedItem as Songs;
            string path = Mp3Info.GetFilePath(s.Title, recievedArtist);
            
            (App.Current as App).nowPlaying_path = path;
            nowPlaying_Page page = new nowPlaying_Page(true);
            NavigationService.Navigate(page);*/

            List<string> paths = new List<string>();
            foreach (var item in song_list.Items)
            {
                var s = item as Songs;
                paths.Add(Mp3Info.GetFilePath(s.Title, recievedArtist));
            }
            (App.Current as App).paths = paths;
            (App.Current as App).startIndex = song_list.SelectedIndex;
            nowPlaying_Page page = new nowPlaying_Page(true);
            NavigationService.Navigate(page);
        }
        private void loadAlbums()
        {
            List<Albums> list = new List<Albums>();

            SQLiteConnection connection = new SQLiteConnection("Data Source=database.db;Version=3;New=False;Compress=True;");
            connection.Open();
            SQLiteCommand command = connection.CreateCommand();

            command.CommandText = "select Album, year from music group by Album having artist='" + recievedArtist + "';";
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                string album = reader.GetString(0);                
                string year = reader.GetInt32(1).ToString();
                if (year == "0")
                    year = null;
                list.Add(new Albums() { Album = album, Year = year });
            }
            reader.Close();
            foreach (var item in list)
            {
                command.CommandText = "select sum(duration) from music where Album = '" + item.Album + "';";
                reader = command.ExecuteReader();
                if(reader.Read())
                    item.Duration = (new TimeSpan(0, 0, reader.GetInt32(0))).ToString();
                reader.Close();

                command.CommandText = "select count(*) from music where Album = '" + item.Album + "';";
                reader = command.ExecuteReader();
                if (reader.Read())
                    item.Tracks = reader.GetInt32(0).ToString();
                reader.Close();
            }
            album_list.ItemsSource = list;
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(album_list.ItemsSource);
            view.SortDescriptions.Add(new SortDescription("Album", ListSortDirection.Ascending));
            connection.Close();

        }
        class Albums
        {
            public string Album { get; set; }           
            public string Year { get; set; }
            public string Tracks { get; set; }
            public string Duration { get; set; }
        }
        private void back_Button_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void album_list_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            var s = album_list.SelectedItem as Albums;
            AlbumPage page = new AlbumPage(s.Album,recievedArtist);
            NavigationService.Navigate(page);
        }
    }
}
