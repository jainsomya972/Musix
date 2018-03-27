using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Finisar.SQLite;

namespace Musix
{
    class DatabaserSeeker
    {
        public static List<string> GetArtists(string database, string tableName)
        {
            List<string> loadedArtists = new List<string>();
            SQLiteConnection connection = new SQLiteConnection("Data Source=" + database + ";Version=3;New=False;Compress=False;");
            connection.Open();
            SQLiteCommand command = connection.CreateCommand();

            command.CommandText = "select distinct Artist from " + tableName;
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                loadedArtists.Add(reader.GetString(0));
            }
            connection.Close();
            loadedArtists.Sort();
            return loadedArtists;
        }
        public static List<string> GetGenres(string database, string tableName)
        {
            List<string> loadedGenres = new List<string>();
            SQLiteConnection connection = new SQLiteConnection("Data Source=" + database + ";Version=3;New=False;Compress=True;");
            connection.Open();
            SQLiteCommand command = connection.CreateCommand();

            command.CommandText = "select distinct Genre from " + tableName;
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                loadedGenres.Add(reader.GetString(0));
            }
            connection.Close();
            loadedGenres.Sort();
            return loadedGenres;
        }
        public static List<string> TitleSearch(string database, string tableName, string searchString)
        {
            List<string> songs = new List<string>();
            SQLiteConnection connection = new SQLiteConnection("Data Source=database.db;Version=3;New=False;Compress=False;");
            connection.Open();
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "select Title from music where Title like '%" + searchString + "%';";
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                songs.Add(reader.GetString(0));
            }
            connection.Close();
            return songs;
        }
    }
}
