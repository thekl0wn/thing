using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace thing
{
    internal class RepositoryThing : Thing, IRepositoryThing
    {

        internal RepositoryThing() : base(TypeThing.ID_REPOSITORY, RepositoryThing.ID_NEW) { }
        internal RepositoryThing(Guid id) : base(id) { }

        public static Guid ID_NEW
        {
            get
            {
                if (_ID_NEW == Guid.Empty) _ID_NEW = RepositoryThing.GetId("NEW");
                return _ID_NEW;
            }
        }
        private static Guid _ID_NEW = Guid.Empty;

        public static Guid ID_SYSTEM
        {
            get
            {
                if (_ID_SYSTEM == Guid.Empty) _ID_SYSTEM = RepositoryThing.GetId("SYSTEM");
                return _ID_SYSTEM;
            }
        }
        private static Guid _ID_SYSTEM = Guid.Empty;

        public static Guid GetId (string code) { return Thing.GetKnownId("REPOSITORY", code); }

    }

    public interface IRepositoryThing : IThing
    {

    }

}
