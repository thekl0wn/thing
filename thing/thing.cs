using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace thing
{

    public class Thing : IThing
    {

        protected Thing(Guid id, bool id_is_type)
        {
            if (id_is_type)
            {
                this.TypeId = id;
                this.Id = Guid.NewGuid();
                this.IsChanged = true;
                this.IsNew = true;
            }
            else
            {
                this.Id = id;
                this.RefreshData();
            }
        }

        public Guid Id { get; } = Guid.Empty;
        public virtual Guid TypeId { get; } = Guid.Empty;
        public bool IsChanged
        {
            get
            {
                if (this.IsNew) _IsChanged = true;
                return _IsChanged;
            }
            set
            {
                if (this.IsNew)
                {
                    _IsChanged = true;
                    return;
                }
                
                _IsChanged = value;
            }
        }
        private bool _IsChanged = false;
        public bool IsNew { get; private set; } = false;

        protected bool Error(Exception e) { return this.Error(e.Message, "Exception Error..."); }
        protected bool Error(string error) { return this.Error(error, "Error..."); }
        protected bool Error(string error, string title)
        {
            // set status
            this.Status(error);

            // trigger error event
            var args = new ErrorEventArgs(error);
            var handler = this.OnError;
            handler?.Invoke(this, args);

            // always return false
            return false;
        }
        protected void Status(string status, bool continue_processing = true)
        {
            // trigger message event
            var args = new StatusEventArgs(status, continue_processing);
            var handler = this.OnStatus;
            handler?.Invoke(this, args);
        }
        protected bool ValidationFail(string message)
        {
            // send as error message
            return this.Error(message, "Validation Failed...");
        }

        protected bool Changed(string old_value, string new_value)
        {
            // check if actually changed
            if (old_value == new_value) return false;
            if (!this.IsChanged) this.IsChanged = true;

            // event
            var args = new PropertyChangeArgs(old_value, new_value);
            var handler = this.OnChanged;
            handler?.Invoke(this, args);

            // default
            return true;
        }
        protected bool Changed(int old_value, int new_value) { return this.Changed(old_value.ToString(), new_value.ToString()); }
        protected bool Changed(bool old_value, bool new_value) { return this.Changed(old_value.ToString(), new_value.ToString()); }
        protected bool Changed(decimal old_value, decimal new_value) { return this.Changed(old_value.ToString(), new_value.ToString()); }
        protected bool Changed(Guid old_value, Guid new_value) { return this.Changed(old_value.ToString(), new_value.ToString()); }
        protected bool Changed(DateTime old_value, DateTime new_value)
        {
            var old_date = old_value.ToShortDateString() + " " + old_value.ToShortTimeString();
            var new_date = new_value.ToShortDateString() + " " + new_value.ToShortTimeString();
            return this.Changed(old_date, new_date);
        }

        public bool RefreshData()
        {
            // default
            return true;
        }

        protected Queue<string> GetInserts()
        {
            var queue = new Queue<string>();

            return queue;
        }
        protected Queue<string> GetUpdates()
        {
            var queue = new Queue<string>();

            return queue;
        }
        

        public bool SaveData() { return this.SaveData(true); }
        public bool SaveData(bool validate)
        {
            // validate ?
            if (validate)
            {
                if (!this.ValidateData()) return false;
            }

            // save
            var queue = new Queue<string>();
            if (this.IsNew)
            {
                queue = this.GetInserts();
            }
            else
            {
                queue = this.GetUpdates();
            }


            // database.execute(queue)

            // remove flags
            this.IsNew = false;
            this.IsChanged = false;

            // default
            return true;
        }

        public bool ValidateData()
        {
            // ensure type id was set
            if (this.TypeId != Guid.Empty) return this.ValidationFail("Type ID is not set.");

            // default
            return true;
        }

        public event EventHandler<ErrorEventArgs> OnError;
        public event EventHandler<PropertyChangeArgs> OnChanged;
        public event EventHandler<StatusEventArgs> OnStatus;
        public event EventHandler<ErrorEventArgs> OnValidationFail;

    }

    public interface IThing : IData, IStatusHandling
    {
        Guid Id { get; }
        Guid TypeId { get; }
    }

}
