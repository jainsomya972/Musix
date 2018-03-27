﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Windows;

namespace Musix
{
    public class LyricsWikia
    {
        private const string _url = "http://lyrics.wikia.com/wiki/";
        private int _error;

        public int Error { get { return _error; } }

        public Uri _uri;
        public LyricsWikia(string artist, string title)
        {
            // http://www.azlyrics.com/lyrics/youngthug/richniggashit.htm

            //removing website tags
            int index = title.IndexOf(" - ");
            if (index > 0)
                title = title.Remove(index);

            //replacing spaces with underscores
            while (artist.Contains(" "))
            {
                artist = artist.Replace(" ", "_");
            }
            while (title.Contains(" "))
            {
                title = title.Replace(" ", "_");
            }     

            _uri = new Uri(_url + artist + ":" + title , UriKind.Absolute);
            //MessageBox.Show(_uri.ToString());
        }

        public string GetLyris()
        {
            string lyrics = string.Empty;
            using (var webClient = new AzLyricsWebClient())
            {
                webClient.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 6.3) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/35.0.1916.114 Safari/537.36");
                webClient.Encoding = Encoding.UTF8;
                /*try
                {
                    var date = webClient.DownloadString(_uri);
                    //var date = Encoding.UTF8.GetString(webClient.DownloadData(_uri));
                    lyrics = ExtractLyricsFromHtml(date);
                }
                catch (WebException ex)
                {
                    _error++;
                }*/
                var date = webClient.DownloadString(_uri);
                //var date = Encoding.UTF8.GetString(webClient.DownloadData(_uri));
                lyrics = ExtractLyricsFromHtml(date);
            }
            return lyrics;
        }

        private string ExtractLyricsFromHtml(string htmlPage)
        {
            const string find1 = "<div class='lyricbox'>";
            var idx = htmlPage.IndexOf(find1, StringComparison.Ordinal);
            if (idx > 0)
            {
                // Remove from start to "<!-- AddThis Button END -->" length
                htmlPage = htmlPage.Remove(0, idx + find1.Length).TrimStart();
                idx = htmlPage.IndexOf("<div class='lyricsbreak'>", StringComparison.Ordinal);
                if (idx > 0)
                {
                    htmlPage = htmlPage.Remove(idx).TrimEnd();
                    htmlPage = WebUtility.HtmlDecode(htmlPage);
                }
            }
            return RemoveAllHtmlTags(htmlPage).Trim();
        }

        private string RemoveAllHtmlTags(string html)
        {
            var idx = html.IndexOf("<form id=\"addsong\"");
            if (idx > 20)
            {
                html = html.Substring(0, idx);
            }

            //adding lines
            while(html.Contains("<br />"))
            {
                html = html.Replace("<br />", "\n");
            }

            //html = StringUtils.RemoveHtmlTags(html);           

            // fix recursive line-break
            //while (html.Contains("\r\n\r\n\r\n"))
             //   html = html.Replace("\r\n\r\n\r\n", "\r\n\r\n");
            return html;
        }

        private bool IsValidUri(string url)
        {
            return !((string.IsNullOrWhiteSpace(url)) && (!Uri.TryCreate(url, UriKind.Absolute, out _uri)));
        }

    }
}


