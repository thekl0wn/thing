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
            DB.OnStatus += Thingy.SetStatus;
            DB.OnError += Thingy.SetError;
        }

        public static IThingDatabase Database { get { return Thingy.DB; } }
        internal static ThingDatabase DB { get; }

        public static bool Refresh()
        {
            if (!Things.Refresh()) return false;

            // default
            return true;
        }

        private static void SetStatus(object sender, StatusEventArgs args)
        {
            // set status
            var handler = Thingy.OnStatus;
            handler?.Invoke(sender, args);

            // set as loggable event
            var log_args = new LoggableEventArgs(args.Message);
            SetLoggableEvent(sender, log_args);
        }
        private static void SetError(object sender, ErrorEventArgs args)
        {
            // set status
            var sts_args = new StatusEventArgs(args.Message);
            SetStatus(sender, sts_args);

            // trigger error event
            var handler = OnError;
            handler?.Invoke(sender, args);
        }
        private static void SetLoggableEvent(object sender, LoggableEventArgs args)
        {
            var handler = Thingy.OnLoggable;
            handler?.Invoke(sender, args);
        }

        public static event EventHandler<StatusEventArgs> OnStatus;
        public static event EventHandler<ErrorEventArgs> OnError;
        public static event EventHandler<LoggableEventArgs> OnLoggable;

        public static class Things
        {

            static Things()
            {
                AllThings = new List<Thing>();
            }

            public static List<IThing> All
            {
                get
                {
                    var list = new List<IThing>();
                    foreach(var item in AllThings)
                    {
                        list.Add(item);
                    }
                    return list;
                }
            }
            private static List<Thing> AllThings { get; }

            public static List<IRepositoryThing> Repositories
            {
                get
                {
                    var list = new List<IRepositoryThing>();
                    foreach(var item in All)
                    {
                        if (item.TypeId == TypeThing.ID_REPOSITORY) list.Add((IRepositoryThing)item);
                    }
                    return list;
                }
            }
            public static List<ITypeThing> Types
            {
                get
                {
                    var list = new List<ITypeThing>();
                    foreach(var item in All)
                    {
                        if(item.TypeId == TypeThing.ID_TYPE) list.Add((ITypeThing)item);
                    }
                    return list;
                }
            }

            public static bool Refresh()
            {
                // clear thing listing
                AllThings.Clear();

                // arrays
                var ids = new List<Guid>();
                var type_ids = new List<Guid>();

                // get list of ids
                var sql = "SELECT [Id], [TypeId] FROM [thing].[Master]";
                if (!DB.StartReader(sql)) return false;
                while (DB.Reader.Read())
                {
                    ids.Add(DB.Reader.GetGuid(0));
                    type_ids.Add(DB.Reader.GetGuid(1));
                }

                // disconnect
                DB.Disconnect();

                // loop through and create based on type
                for(int i = 0; i < ids.Count; i++)
                {
                    if (type_ids[i] == TypeThing.ID_TYPE)
                    {
                        var t = new TypeThing(ids[i]);
                        AllThings.Add(t);
                    }
                    else if (type_ids[i] == TypeThing.ID_PROPERTY)
                    {
                        var p = new PropertyThing(ids[i]);
                        AllThings.Add(p);
                    }
                    else if (type_ids[i] == TypeThing.ID_STATUS)
                    {
                        var s = new StatusThing(ids[i]);
                        AllThings.Add(s);
                    }
                    else if(type_ids[i] == TypeThing.ID_REPOSITORY)
                    {
                        var r = new RepositoryThing(ids[i]);
                        AllThings.Add(r);
                    }
                }

                // default
                return true;
            }

        }

    }

}
