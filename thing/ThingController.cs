using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace thing
{

    internal class ThingController : List<Thing>, IThingController
    {

        public List<IThing> All
        {
            get
            {
                var list = new List<IThing>();
                foreach(var thing in this)
                {
                    list.Add(thing);
                }
                return list;
            }
        }

        public event EventHandler<ErrorEventArgs> OnError;
        public event EventHandler<StatusEventArgs> OnStatus;

    }

    public interface IThingController
    {
        List<IThing> All { get; }

        event EventHandler<ErrorEventArgs> OnError;
        event EventHandler<StatusEventArgs> OnStatus;
    }

}
