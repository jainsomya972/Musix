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

namespace Musix
{
    /// <summary>
    /// Interaction logic for GenrePage.xaml
    /// </summary>
    public partial class GenrePage : Page
    {
        public GenrePage(string genre)
        {
            InitializeComponent();
            recievedGenre = genre;
        }
        string recievedGenre;

        private void loadSongs()
        {
            List<Songs> list = new List<Songs>();

            SQLiteConnection connection = new SQLiteConnection("Data Source=database.db;Version=3;New=False;Compress=True;");
            connection.Open();
            SQLiteCommand command = connection.CreateCommand();

            command.CommandText = "select Title, Album, Artist, duration from music where genre='" + recievedGenre + "';";
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                string title = reader.GetString(0);
                if (title == null || title == "")
                {
                    title = "unknown title";
                }                
                string duration = new TimeSpan(0, 0, reader.GetInt32(3)).ToString(@"mm\:ss");

                list.Add(new Songs() { Title = title, Album = reader.GetString(1), Artist=reader.GetString(2), Duration = duration });
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
            public string Artist { get; set; }
            public string Duration { get; set; }
        }
        private void songList_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            /*var s = song_list.SelectedItem as Songs;
            string path = Mp3Info.GetFilePath(s.Title);
            (App.Current as App).nowPlaying_path = path;
            nowPlaying_Page page = new nowPlaying_Page(true);
            NavigationService.Navigate(page);
            */
            List<string> paths = new List<string>();
            foreach (var item in song_list.Items)
            {
                var s = item as Songs;
                paths.Add(Mp3Info.GetFilePath(s.Title));
            }
            (App.Current as App).paths = paths;
            (App.Current as App).startIndex = song_list.SelectedIndex;
            nowPlaying_Page page = new nowPlaying_Page(true);
            NavigationService.Navigate(page);
        }

        private void back_Button_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void Page_load(object sender, RoutedEventArgs e)
        {
            Genre_label.Content = recievedGenre;
            loadSongs();
        }
    }
}
