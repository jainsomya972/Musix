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
using System.ComponentModel;
using Finisar.SQLite;

namespace Musix
{
    /// <summary>
    /// Interaction logic for AlbumPage.xaml
    /// </summary>
    public partial class AlbumPage : Page
    {
        public AlbumPage(string album,string artist)
        {
            InitializeComponent();
            recievedAlbum = album;
            recievedArtist = artist;

        }
        string recievedAlbum, recievedArtist;

        private void Page_load(object sender, RoutedEventArgs e)
        {
            Album_label.Content = recievedAlbum;
            Artist_label.Content = recievedArtist;
            try
            {
                string path = Mp3Info.GetFilePath(recievedAlbum, 0);
                AlbumArt.Source = Mp3Info.GetAlbumArt(path);
            }
            catch { }
            loadSongs();
        }

        private void loadSongs()
        {
            List<Songs> list = new List<Songs>();

            SQLiteConnection connection = new SQLiteConnection("Data Source=database.db;Version=3;New=False;Compress=True;");
            connection.Open();
            SQLiteCommand command = connection.CreateCommand();

            command.CommandText = "select Title, duration from music where album='" + recievedAlbum + "';";
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                string title = reader.GetString(0);                
                string duration = new TimeSpan(0, 0, reader.GetInt32(1)).ToString(@"mm\:ss");

                list.Add(new Songs() { Title = title, Duration = duration });
            }
            song_list.ItemsSource = list;
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(song_list.ItemsSource);
            view.SortDescriptions.Add(new SortDescription("Title", ListSortDirection.Ascending));
            connection.Close();

        }
        class Songs
        {
            public string Title { get; set; }            
            public string Duration { get; set; }
        }

        private void back_Button_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void song_list_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            /*var s = song_list.SelectedItem as Songs;
            string path = Mp3Info.GetFilePath(s.Title, recievedAlbum,0);
            (App.Current as App).nowPlaying_path = path;
            nowPlaying_Page page = new nowPlaying_Page(false);
            NavigationService.Navigate(page);
            */
            List<string> paths = new List<string>();
            foreach (var item in song_list.Items)
            {
                var s = item as Songs;
                paths.Add(Mp3Info.GetFilePath(s.Title, recievedAlbum,0));
            }
            if((App.Current as App).paths!=null)
            (App.Current as App).paths.Clear();
            (App.Current as App).paths = paths;
            (App.Current as App).startIndex = song_list.SelectedIndex;
            nowPlaying_Page page = new nowPlaying_Page(true);
            NavigationService.Navigate(page);
        }
    }
}
