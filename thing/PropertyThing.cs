using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace thing
{
    public class PropertyThing : Thing
    {

        internal PropertyThing() : base(TypeThing.ID_PROPERTY, RepositoryThing.ID_NEW) { }
        internal PropertyThing(Guid id) : base(id) { }

        public static Guid ID_CODE
        {
            get
            {
                if (_ID_CODE == Guid.Empty) _ID_CODE = PropertyThing.GetId("CODE");
                return _ID_CODE;
            }
        }
        private static Guid _ID_CODE = Guid.Empty;

        public static Guid ID_DESCRIPTION
        {
            get
            {
                if (_ID_DESCRIPTION == Guid.Empty) _ID_DESCRIPTION = PropertyThing.GetId("DESCRIPTION");
                return _ID_DESCRIPTION;
            }
        }
        private static Guid _ID_DESCRIPTION = Guid.Empty;

        public static Guid ID_NAME
        {
            get
            {
                if (_ID_NAME == Guid.Empty) _ID_NAME = PropertyThing.GetId("NAME");
                return _ID_NAME;
            }
        }
        private static Guid _ID_NAME = Guid.Empty;

        public static Guid GetId(string code) { return Thing.GetKnownId("PROPERTY", code); }

    }
}
