using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;
using System.Threading.Tasks;

namespace HTTPServer
{
    class HttpResponse
    {
        private string _initialLine;
        private readonly Dictionary<string, string> _headers;
        public IEnumerable<byte> Content;
        private const string RootCatalog = @"c:\temp";

        /// <summary>
        /// HttpResponse class holds data, that will be returned to client, automatically sets Status, Date and Content-Length
        /// </summary>
        /// <param name="requestedUri">URI of the requested file, keep empty or null if no file is requested</param>
        public HttpResponse(string requestedUri)
        {
            _headers = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(requestedUri))
            {
                GetFile(requestedUri);
            }
            SetStatus(200);
            SetDate();
        }


        private void GetFile(string uri)
        {
            var filename = RootCatalog + WebUtility.UrlDecode(uri).Replace('/', '\\');

            if (File.Exists(filename))
            {
                Trace.WriteLine("Reading requested file - " + filename);
                Content = File.ReadAllBytes(filename);
            }
            else if (Directory.Exists(filename))
            {
                Trace.WriteLine("Reading welcome file from requested folder- " + filename);

                if (File.Exists(filename + "index.html"))
                {
                    Content = File.ReadAllBytes(filename + "index.html");
                    filename = filename + "index.html";
                }
                if (File.Exists(filename + "main.html"))
                {
                    Content = File.ReadAllBytes(filename + "main.html");
                    filename = filename + "main.html";
                }
            }
            if (Content.LongCount() > 0)
            {
                AddHeader("Content-Length", Convert.ToString(Content.Count()));
                SetContentType(filename);
                SetStatus(200);
            }
            else
            {
                SetStatus(404);
                Trace.WriteLine("Requested file not found - " + filename);
            }

        }

        private void SetStatus(int statusCode)
        {
            _initialLine = "HTTP/1.0 " + Convert.ToString(statusCode) + " ";

            switch (statusCode)
            {
                case 404:
                    _initialLine += "Not found";
                    break;
                case 501:
                    _initialLine = "Not implemented";
                    break;
                case 200:
                    _initialLine += "OK";
                    break;
            }
            _initialLine += "\r\n";
        }

        /// <summary>
        /// Adds header to response
        /// </summary>
        /// <param name="name">Name of the header, e.g. </param>
        /// <param name="data"></param>
        public void AddHeader(string name, string data)
        {
            _headers.Add(name, data);
        }

        private void SetContentType(string filename)
        {
            var contentType = "";
            var extension = Path.GetExtension(filename);
            if (extension != null)
                switch (extension.ToLower())
                {
                    case ".jpg":
                    case ".jpeg":
                        contentType = "image/jpeg";
                        break;
                    case ".html":
                    case ".htm":
                        contentType = "text/html";
                        break;
                    case ".doc":
                        contentType = "application/msword";
                        break;
                    case ".gif":
                        contentType = "image/gif";
                        break;
                    case ".pdf":
                        contentType = "application/pdf";
                        break;
                    case ".css":
                        contentType = "text/css";
                        break;
                    case ".xml":
                        contentType = "text/xml";
                        break;
                    case ".jar":
                        contentType = "application/x-java-archive";
                        break;
                    default:
                        contentType = "application/octet-stream";
                        break;
                }
            AddHeader("Content-Type", contentType);
        }

        private void SetDate()
        {
            var en = new CultureInfo("en-US");
            Thread.CurrentThread.CurrentCulture = en;
            AddHeader("Date", DateTime.UtcNow.ToString("ddd, dd MMM yyyy HH:mm:ss") + " GMT");
        }

        /// <summary>
        /// Connects together headers, content and converts it to bytes 
        /// </summary>
        /// <returns>Bytes representing complete HTTP response</returns>
        public byte[] ToBytes()
        {
            var response = _headers.Aggregate(_initialLine, (current, header) => current + (header.Key + ": " + header.Value + "\r\n"));
            response += "\r\n";
            IEnumerable<byte> rv = Encoding.UTF8.GetBytes(response);
            if ((Content != null) && (Content.Any()))
            {
                rv = rv.Concat(Content);
            }
            var a = rv.ToArray();
            return a;
        }
    }
}
