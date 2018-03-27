using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Windows;
using Finisar.SQLite;

namespace Musix
{
    class DatabaseBuilder
    {        
        public string path { get; set; }
        private string DBname = "database.db";
        public DatabaseBuilder(string folderPath)
        {
            path = folderPath;
        }


        public void createDatabase()
        {
            var fileList = listAllFilePaths();

            //creating log file for all music file paths
            File.WriteAllLines("files.mus", fileList);

            //creating database using sqlite
            SQLiteConnection connection = new SQLiteConnection("Data Source=database_temp.db;Version=3;New=True;Compress=False;");
            connection.Open();
            SQLiteCommand command = connection.CreateCommand();
            //creating table
            command.CommandText = "CREATE TABLE music (id integer , Title varchar(30),Album varchar(30),Artist varchar(30),Genre varchar(30),duration integer, year varchar(5))";
            command.ExecuteNonQuery();
            //inserting file information
            int i = 0;
            foreach (var file in fileList)
            {
                try
                {
                    Mp3Info info = new Mp3Info(file);
                    command.CommandText = "INSERT INTO music VALUES (" + i + ",'" + info.Title + "', '" + info.Album + "', '" + info.Artist + "', '" + info.Genre + "', " + info.Duration + ", " + info.Year.ToString() + ");";
                    command.ExecuteNonQuery();
                }
                catch { }               
                i++;
            }
            connection.Close();
            //replacing original data file with temporary file (newly created)
            if (File.Exists("database.db"))
                File.Delete("database.db");
            File.Move("database_temp.db", "database.db");
        }
        
        private List<string> listAllFilePaths()
        {
            List<string> fileList = new List<string>();
            string[] files,dirs;
            string currentPath = path;
            Queue<string> q = new Queue<string>();
            q.Enqueue(currentPath);
            while(q.Count>0)
            {
                currentPath = q.Dequeue();
                files = Directory.GetFiles(currentPath);
                dirs = Directory.GetDirectories(currentPath);
                foreach (var dir in dirs)                
                    q.Enqueue(dir);
                foreach(var file in files)
                {
                    FileInfo f = new FileInfo(file);
                    if (f.Extension == ".mp3")
                        fileList.Add(file);
                }
            }
            return fileList;
        }
    }
}
