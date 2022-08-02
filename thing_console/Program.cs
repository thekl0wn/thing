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
            thing.DB.DatabaseName = "Thing_db";
            thing.DB.ServerName = "EV701307DELL\\LMR";
            thing.DB.TestConnection();

            // finished
            Console.WriteLine("press any key to exit");
            Console.ReadKey();
        }
    }
}
