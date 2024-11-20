using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Hosting;
using Owin;
using Microsoft.Owin.Cors;
using System.Threading;

namespace SelfHostedWebScanner
{
    class Program
    {

        //public static ScannerBroadcaster s;
        //public static Printer p;

        static void Main(string[] args)
        {
            string url = "http://localhost:8080";
            using (WebApp.Start(url))
            {
                Console.WriteLine("Server running on {0}", url);
                Console.WriteLine("'Q' to quit", url);
                //s = ScannerBroadcaster.Instance;
                //p = Printer.Instance;
               // Console.WriteLine("Printer and scanner created");
                while (true)
                {
                    string f=Console.ReadLine();
                    if (f.ToUpper() == "Q")
                    {
                        break;
                    }
                }
            }
        }
    }

   
}
