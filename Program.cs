using System;

namespace HTTPServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var s = new HttpServer(80);
            Console.ReadLine();
        }
    }
}
