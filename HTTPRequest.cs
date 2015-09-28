using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace HTTPServer
{
    class HttpRequest
    {
        public Dictionary<string, string> Headers { get; private set; }
        public string MessageType { get; private set; }
        public string HttpVersion { get; private set; }
        public string Uri { get; private set; }


        /// <summary>
        /// Parses information from incoming request
        /// </summary>
        /// <param name="headerString">Clients request</param>
        public HttpRequest(string headerString)
        {
            var lines = headerString.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            
            Trace.WriteLine("Request received - " + lines[0]);
            
            var unparsedHeaders = lines.Skip(1).ToArray();
            ParseHeaders(unparsedHeaders);
            ParseInitialLine(lines[0]); 
        }

        private void ParseInitialLine(string initialLine)
        {
            var initialLineSplit = initialLine.Split(new[] { ' ' }, 3);
            
            MessageType = initialLineSplit[0].ToUpper().Trim();
            if (initialLineSplit.Length > 1) Uri = initialLineSplit[1];
            if (initialLineSplit.Length > 2) HttpVersion = initialLineSplit[2];
        }

        private void ParseHeaders(IEnumerable<string> unparsedHeaders)
        {
            Headers = new Dictionary<string, string>();
            foreach (var pieces in unparsedHeaders.Select(header => header.Split(new[] { ':' }, 2)).Where(pieces => pieces.Length == 2))
            {
                Headers.Add(pieces[0].Trim(), pieces[1].Trim());
            }
        }
    }
}
