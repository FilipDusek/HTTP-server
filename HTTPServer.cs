using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;

namespace HTTPServer
{
    public class HttpServer
    {
        public volatile bool StopServer;
        public HttpServer(int port)
        {
            var server = new TcpListener(port);
            server.Start();
            Trace.WriteLine("Server started");
            try
            {
                StopServer = false;
                do
                {
                    Trace.WriteLine("Waiting for client...");
                    var connectionSocket = server.AcceptSocket();
                    Trace.WriteLine("Client connected");
                    ThreadPool.QueueUserWorkItem(HandleRequest, connectionSocket);
                } while (!StopServer);
            }
            finally
            {
                server.Stop();
                Trace.WriteLine("Server stopped");
            }
        }


        private void HandleRequest(object conSocket)
        {
            var connectionSocket = (Socket)conSocket;
            Stream ns = new NetworkStream(connectionSocket);
            var sr = new StreamReader(ns);
            var sw = new StreamWriter(ns);

            try
            {
                var line = "dummy";
                var requestRaw = "";
                while (line != "\r\n")
                {
                    line = sr.ReadLine() + "\r\n";
                    requestRaw += line;
                }

                var request = new HttpRequest(requestRaw);
                var response = new HttpResponse(request.Uri);

                StopServer = request.MessageType == "EXIT";

                var responseBytes = response.ToBytes();
                ns.Write(responseBytes, 0, responseBytes.Length);
            }
            finally
            {
                sw.Close();
                sr.Close();
                ns.Close();
                connectionSocket.Close();
            }
        }
    }
}
