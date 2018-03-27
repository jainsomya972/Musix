using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Finisar.SQLite;
using System.IO;
using System.Windows.Media.Imaging;

namespace Musix
{
    public class Mp3Info
    {        
        public string Title { get; }
        public string Album { get; }
        public string Artist { get; }
        public string Genre { get; }
        public int Duration { get; }
        public string Year { get; }
        public Mp3Info(string path)
        {
            TagLib.File tagFile = TagLib.File.Create(path); 
            try
            {
                Title = tagFile.Tag.Title.Replace('\'', ' ');
                if (Title == "" || Title == null)
                    Title = "unknown Title";
            } catch { }
            try
            {
                Album = tagFile.Tag.Album.Replace('\'', ' ');
                if (Album == "" || Album == null)
                    Album = "Unknown Album";

            } catch { }
            try
            {
                Artist = tagFile.Tag.FirstAlbumArtist.Replace('\'', ' ');
                if (Artist == ""||Artist==null)
                    Artist = "Unknown Artist";
            }
            catch { }
            try { Year = tagFile.Tag.Year.ToString(); } catch { }
            try { Genre = tagFile.Tag.FirstGenre.Replace('\'', ' '); } catch { }           
            Duration = (int)tagFile.Properties.Duration.TotalSeconds;                        
        }             
        public static string GetFilePath(string title,string artist)
        {
            //getting selected song's corresponding index no. as 'id'
            SQLiteConnection connection = new SQLiteConnection("Data Source=database.db;Version=3;New=False;Compress=True;");
            connection.Open();
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "select id from music where Title='" + title + "' and Artist='" + artist + "';";
            SQLiteDataReader reader = command.ExecuteReader();
            reader.Read();
            int id = reader.GetInt32(0);
            connection.Close();

            //getting selected song's corresponding filepath using 'id' from files.mus
            StreamReader stream = new StreamReader("files.mus");
            string path = "";
            for (int i = 0; i <= id; i++)
            {
                path = stream.ReadLine();
            }
            return path;
        }
        public static string GetFilePath(string title)
        {
            //getting selected song's corresponding index no. as 'id'
            SQLiteConnection connection = new SQLiteConnection("Data Source=database.db;Version=3;New=False;Compress=True;");
            connection.Open();
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "select id from music where Title='" + title + "';";
            SQLiteDataReader reader = command.ExecuteReader();
            reader.Read();
            int id = reader.GetInt32(0);
            connection.Close();

            //getting selected song's corresponding filepath using 'id' from files.mus
            StreamReader stream = new StreamReader("files.mus");
            string path = "";
            for (int i = 0; i <= id; i++)
            {
                path = stream.ReadLine();
            }
            return path;
        }
        public static string GetFilePath(string title,string album,int extra)
        {
            //getting selected song's corresponding index no. as 'id'
            SQLiteConnection connection = new SQLiteConnection("Data Source=database.db;Version=3;New=False;Compress=True;");
            connection.Open();
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "select id from music where Title='" + title + "' and album='"+album+"';";
            SQLiteDataReader reader = command.ExecuteReader();
            reader.Read();
            int id = reader.GetInt32(0);
            connection.Close();

            //getting selected song's corresponding filepath using 'id' from files.mus
            StreamReader stream = new StreamReader("files.mus");
            string path = "";
            for (int i = 0; i <= id; i++)
            {
                path = stream.ReadLine();
            }
            return path;
        }
        public static string GetFilePath(string album, int extra)
        {
            //getting selected song's corresponding index no. as 'id'
            SQLiteConnection connection = new SQLiteConnection("Data Source=database.db;Version=3;New=False;Compress=True;");
            connection.Open();
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "select id from music where album='" + album + "';";
            SQLiteDataReader reader = command.ExecuteReader();
            reader.Read();
            int id = reader.GetInt32(0);
            connection.Close();

            //getting selected song's corresponding filepath using 'id' from files.mus
            StreamReader stream = new StreamReader("files.mus");
            string path = "";
            for (int i = 0; i <= id; i++)
            {
                path = stream.ReadLine();
            }
            return path;
        }
        public static BitmapImage GetAlbumArt(string path)
        {
            TagLib.File song = TagLib.File.Create(path);
            MemoryStream ms = new MemoryStream(song.Tag.Pictures[0].Data.Data);
            ms.Seek(0, SeekOrigin.Begin);
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.StreamSource = ms;
            bitmap.EndInit();
            return bitmap;
        }       
    }
}
