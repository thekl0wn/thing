using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using thing;

namespace thing_console
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // setup
            Thingy.OnError += OnError;
            Thingy.OnStatus += OnStatus;
            Thingy.Database.DatabaseName = "Thing_db";
            Thingy.Database.ServerName = "EV701307DELL\\LMR";
            Thingy.Database.TestConnection();

            // test
            Thingy.Refresh();
            foreach(var item in Thingy.Things.Types)
            {
                Console.WriteLine(item.Name);
            }
            //var id = TypeThing.ID_TYPE;
            //var t = new TypeThing(id);
            //Console.WriteLine(t.Id);
            //Console.WriteLine(t.Name);
            //t.Description = "";
            //t.Save();
            //t.Dispose();
            
            // finished
            Console.WriteLine("press any key to exit");
            Console.ReadKey();
        }

        private static void OnStatus(object sender, StatusEventArgs args)
        {
            Console.WriteLine(args.Message);
        }
        private static void OnError(object sender, ErrorEventArgs args)
        {
            var clr = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(args.Message);
            Console.ForegroundColor = clr;
        }
    }
}
