using System;
using System.Text;

namespace Proto01
{
    class Program
    {
        static void PrintRoutes(Router rt)
        {
            Console.WriteLine("Router: {0}", rt.Id);
            foreach (var info in rt.routes)
            {
                var routeInfo = info.Value;
                Console.WriteLine("{0}: jump to {1}, length {2}", routeInfo.TargetId, routeInfo.NextId, routeInfo.Length);
            }
        }

        static void Main(string[] args)
        {
            var rt1 = new Router("rt1");
            var rt2 = new Router("rt2");

            var ln1 = new RouterLink(rt1, rt2);

            PrintSplitter();
            PrintRoutes(rt1);
            PrintRoutes(rt2);

            var rt3 = new Router("rt3");
            var ln2 = new RouterLink(rt1, rt3);

            PrintSplitter();
            PrintRoutes(rt1);
            PrintRoutes(rt2);
            PrintRoutes(rt3);

            var rt4 = new Router("rt4");
            var ln3 = new RouterLink(rt2, rt4);

            PrintSplitter();
            PrintRoutes(rt1);
            PrintRoutes(rt2);
            PrintRoutes(rt3);
            PrintRoutes(rt4);

            var rt5 = new Router("rt5");
            var ln4 = new RouterLink(rt2, rt5);

            PrintSplitter();
            PrintRoutes(rt1);
            PrintRoutes(rt2);
            PrintRoutes(rt3);
            PrintRoutes(rt4);
            PrintRoutes(rt5);

            PrintSplitter();

            var message = rt5.BuildMessage("rt3");
            message.Label = "hello, rt3!";
            message.Send();

            // check the rt3 inbox for messages
            Console.WriteLine("rt3 inbox size is: {0}", rt3.Inbox.Count);

            Console.WriteLine("press enter...");
            Console.ReadLine();

            // now routes must be : 
            //
            //               +-- r4
            //               |
            //  r3 -- r1 -- r2
            //               |
            //               +-- r5


        }

        private static void PrintSplitter()
        {
            Console.WriteLine("-----------------------------------------------------");
        }
    }
}
