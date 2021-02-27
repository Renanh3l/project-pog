using System;

namespace PogServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "POG Game Server";

            Server.Start(20, 4001);

            Console.ReadKey();
        }
    }
}
