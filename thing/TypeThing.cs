using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace thing
{
    public class TypeThing : Thing
    {

        protected TypeThing() : base(TypeThing.ID_TYPE, true) { }
        protected TypeThing(Guid id) : base(id, false) { }


        public static Guid ID_PROPERTY
        {
            get
            {
                if (_ID_PROPERTY == Guid.Empty) _ID_PROPERTY = TypeThing.GetId("PROPERTY");
                return _ID_PROPERTY;
            }
        }
        private static Guid _ID_PROPERTY = Guid.Empty;

        public static Guid ID_STATUS
        {
            get
            {
                if (_ID_STATUS == Guid.Empty) _ID_STATUS = TypeThing.GetId("STATUS");
                return _ID_STATUS;
            }
        }
        private static Guid _ID_STATUS = Guid.Empty;

        public static Guid ID_TYPE
        {
            get
            {
                if (_ID_TYPE == Guid.Empty) _ID_TYPE = TypeThing.GetId("TYPE");
                return _ID_TYPE;
            }
        }
        private static Guid _ID_TYPE = Guid.Empty;

        public static Guid GetId(string code)
        {
            return Thing.GetKnownId("TYPE", code);
        }

    }
}
