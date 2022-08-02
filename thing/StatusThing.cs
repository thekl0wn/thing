using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace thing
{
    public class StatusThing : Thing
    {

        internal StatusThing() : base(TypeThing.ID_STATUS, RepositoryThing.ID_NEW) { }
        internal StatusThing(Guid id) : base(id) { }

        public static Guid ID_ACTIVE
        {
            get
            {
                if (_ID_ACTIVE == Guid.Empty) _ID_ACTIVE = StatusThing.GetId("ACTIVE");
                return _ID_ACTIVE;
            }
        }
        private static Guid _ID_ACTIVE = Guid.Empty;

        public static Guid ID_DEACTIVATED
        {
            get
            {
                if (_ID_DEACTIVATED == Guid.Empty) _ID_DEACTIVATED = StatusThing.GetId("DEACTIVATED");
                return _ID_DEACTIVATED;
            }
        }
        private static Guid _ID_DEACTIVATED = Guid.Empty;

        public static Guid GetId(string code) { return Thing.GetKnownId("STATUS", code); }

    }
}
