using System;
using System.IO;
using System.Net;
using System.Text;

namespace Nonogram
{
    enum Source
    {
        WebPBN
    }

    class Scrapper
    {
        HttpWebRequest request;

        public Scrapper()
        {
            
        }

        public void GetFromSource(Source source, int id, TextWriter writer)
        {
            switch(source)
            {
                case Source.WebPBN:
                    request = (HttpWebRequest)WebRequest.Create("https://webpbn.com/export.cgi");
                    request.Credentials = CredentialCache.DefaultCredentials;
                    request.Method = "POST";

                    string postData = $"fmt=xml&go=1&id={id}";
                    byte[] byteArray = Encoding.UTF8.GetBytes(postData);

                    request.ContentType = "application/x-www-form-urlencoded";
                    request.ContentLength = byteArray.Length;

                    Stream dataStream = request.GetRequestStream();
                    dataStream.Write(byteArray, 0, byteArray.Length);
                    dataStream.Close();

                    WebResponse response = request.GetResponse();

                    // Display the status. TODO: Turn this into some kind of error check...
                    Console.WriteLine(((HttpWebResponse)response).StatusDescription);

                    using (dataStream = response.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(dataStream);
                        string responseFromServer = reader.ReadToEnd();
                        writer.WriteLine(responseFromServer);
                    }
                    response.Close();

                    break;
            }
        }
    }
}
