using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace thing
{
    
    public static class Thingy
    {

        static Thingy()
        {
            // setup database
            DB = new ThingDatabase();
            DB.OnStatus += Thingy.OnStatus;
            DB.OnError += Thingy.OnError;

            // setup the thing controller
            Thingies = new ThingController();
            Thingies.OnStatus += Thingy.OnStatus;
            Thingies.OnError += Thingy.OnError;
        }

        public static IThingDatabase Database { get { return Thingy.DB; } }
        internal static ThingDatabase DB { get; }

        public static IThingController Things { get { return Thingy.Thingies; } }
        internal static ThingController Thingies { get; }

        public static event EventHandler<StatusEventArgs> OnStatus;
        public static event EventHandler<ErrorEventArgs> OnError;

    }

}
