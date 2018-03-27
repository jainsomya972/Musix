using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace Musix
{
    class AzLyricsWebClient : WebClient
    {
        // http://stackoverflow.com/questions/15034771/cant-download-utf-8-web-content
        protected override WebRequest GetWebRequest(Uri address)
        {
            HttpWebRequest req = base.GetWebRequest(address) as HttpWebRequest;
            req.KeepAlive = false;
            if (req != null)
            {
                req.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            }
            return req;
        }
    }

}
