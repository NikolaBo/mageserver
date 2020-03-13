using System;

namespace MageServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "MageServer";

            Server.Start(10, 2239);

            Console.ReadKey();
        }
    }
}
