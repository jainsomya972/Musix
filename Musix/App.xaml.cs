using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Navigation;

namespace Musix
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public Playlist playlist;
        public int startIndex;
        public List<string> paths;
        public string nowPlaying_path { get; set; }
        public MediaPlayer player { get; set; }
        public int nowPlaying_Status { get; set; }// 0 = stopped, 1 = playing, 2 = paused
        public App()
        {
            initializer();
        }
        public void initializer()
        {
            player = new MediaPlayer();
            nowPlaying_Status = 0;
            playlist = null;
            paths = null;
            nowPlaying_path = null;
        }
        protected override void OnStartup(StartupEventArgs e)
        {
            if(AppDomain.CurrentDomain.SetupInformation.ActivationArguments!=null
                && AppDomain.CurrentDomain.SetupInformation.ActivationArguments.ActivationData.Length>0)
            {
                paths = new List<string>();
                foreach (var item in AppDomain.CurrentDomain.SetupInformation.ActivationArguments.ActivationData)
                {
                    string fname = item;
                    Uri uri = new Uri(fname);
                    paths.Add(uri.LocalPath);
                }
                startIndex = 0;
                App.Current.Properties["startupFiles"] = 1;
            }
            base.OnStartup(e);
            

        }
    }
}
