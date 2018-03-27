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
using System.Threading;
using System.Windows.Shapes;
using Finisar.SQLite;
using System.Windows.Forms;
using System.IO;

namespace Musix
{
    /// <summary>
    /// Interaction logic for welcomePage.xaml
    /// </summary>
    public partial class welcomePage : Page
    {
        public welcomePage()
        {
            InitializeComponent();
        }

        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists("folders.mus"))
            {
                waitMessage_label.Content = "Please wait ... its almost done. (Creating database)";               
                /*string path = File.ReadAllText("folders.mus");                
                DatabaseBuilder d = new DatabaseBuilder(path);
                //creating database on another thread                 
                ThreadStart threadStart = new ThreadStart(d.createDatabase);
                Thread th = new Thread(threadStart);
                th.Start();                
                while(true)
                {
                    Thread.Sleep(3000);
                    if (!th.IsAlive)
                        break;
                }*/
                MainPage m = new MainPage();
                NavigationService.Navigate(m);
            }
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog browser = new FolderBrowserDialog();
            DialogResult result = browser.ShowDialog();
            string path = browser.SelectedPath;
            folderPath_textBox.Text = path;
            File.WriteAllText("folders.mus", path);
           
        }

        private void startButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (File.Exists("folders.mus"))
            {
                waitMessage_label.Content = "Please wait ... its almost done. (Creating database)";
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (App.Current.Properties["startupFiles"] != null)
            {
                nowPlaying_Page page = new nowPlaying_Page(true);
                NavigationService.Navigate(page);
                App.Current.Properties["startupFiles"] = null;
            }
        }
    }
}
