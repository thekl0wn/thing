using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace thing
{

    internal class PropertyValue : IPropertyValue
    {

        internal PropertyValue(Guid thing_id, Guid property_id)
        {
            this.ThingId = thing_id;
            this.PropertyId = property_id;
            this.IsNew = false;
            this.IsChanged = false;
            this.Refresh();
        }
        internal PropertyValue(Guid thing_id, Guid property_id, string value, bool is_existing)
        {
            this.ThingId = thing_id;
            this.PropertyId = property_id;
            this.Value = value;
            if (is_existing)
            {
                this.IsNew = false;
                this.IsChanged = false;
            }
            else
            {
                this.IsNew = true;
                this.IsChanged = true;
            }
        }

        public bool IsChanged
        {
            get
            {
                if (this.IsNew)
                {
                    _IsChanged = true;
                    return true;
                }

                return _IsChanged;
            }
            set
            {
                if (this.IsChanged) return;
                if(this.IsChanged==value) return;
                _IsChanged = value;
            }
        }
        private bool _IsChanged = false;

        public bool IsNew { get; private set; } = false;

        public Guid ThingId { get; private set; }
        public Guid PropertyId { get; private set; }

        public string Value
        {
            get { return _Value; }
            set
            {
                if (_Value == value) return;
                this._Value = value;
                var handler = this.OnChanged;
                handler?.Invoke(this, EventArgs.Empty);
            }
        }
        private string _Value = "";

        internal string GetSaveString()
        {
            // variables
            var sql = new StringBuilder();
            var val = this.Value;

            // format value
            val = val.Trim();
            if (string.IsNullOrEmpty(val)) val = "";
            while (val.StartsWith("'")) val = val.Substring(1);
            while (val.EndsWith("'")) val = val.Substring(0, val.Length - 1);
            val = val.Trim();
            val = val.Replace("'", "''");

            // build sql
            if (this.IsNew)
            {
                // insert
                sql.Append("INSERT [thing].[Property] ( [ThingId], [PropertyId], [Value] ) SELECT '");
                sql.Append(this.ThingId);
                sql.Append("', '");
                sql.Append(this.PropertyId);
                sql.Append("', '");
                sql.Append(val);
                sql.Append("'");
            }
            else
            {
                // update
                sql.Append("UPDATE [thing].[Property] SET [Value] = '");
                sql.Append(val);
                sql.Append("' ");
                sql.Append(this.GetWhereClause());
            }

            // return sql
            return sql.ToString();
        }
        private string GetWhereClause()
        {
            var sql = new StringBuilder();
            sql.Append(" WHERE[ThingId] = '");
            sql.Append(this.ThingId);
            sql.Append("' AND [PropertyId] = '");
            sql.Append(this.PropertyId);
            sql.Append("'");

            return sql.ToString();
        }

        public bool Refresh()
        {
            // build sql
            var sql = new StringBuilder();
            sql.Append("SELECT [Value] FROM [thing].[Property] ");
            sql.Append(this.GetWhereClause());

            // pull in data
            string value = "";
            if (!Thingy.DB.Scalar(sql.ToString(), out value)) return false;
            this.Value = value;

            // set flags
            this.IsNew = false;
            this.IsChanged = false;

            // event
            var handler = this.OnRefreshed;
            handler?.Invoke(this, EventArgs.Empty);

            // default
            return true;
        }

        public bool Save() { return this.Save(true); }
        public bool Save(bool validate)
        {
            // check if changed
            if (!this.IsChanged) return true;

            // validate?
            if (validate) if (!this.Validate()) return false;

            // get sql
            var sql = this.GetSaveString();

            // execute the save
            if(!Thingy.DB.Execute(sql)) return false;

            // default
            return this.Saved();
        }
        internal bool Saved()
        {
            // reset flags
            this.IsNew = false;
            this.IsChanged = false;

            // event
            var handler = this.OnSaved;
            handler?.Invoke(this, EventArgs.Empty);

            // default
            return true;
        }

        public bool Validate()
        {
            // default
            return true;
        }

        public event EventHandler OnChanged;
        public event EventHandler OnRefreshed;
        public event EventHandler OnSaved;

    }

    public interface IPropertyValue
    {
        bool IsChanged { get; }
        bool IsNew { get; }
        Guid ThingId { get; }
        Guid PropertyId { get; }
        string Value { get; set; }

        bool Refresh();
        bool Save();
        bool Save(bool validate);
        bool Validate();

        event EventHandler OnChanged;
        event EventHandler OnRefreshed;
        event EventHandler OnSaved;
    }


    internal class PropertyValueList : List<PropertyValue>, IPropertyValueList
    {

        internal PropertyValueList(Guid thing_id)
        {
            ThingId = thing_id;
        }

        public Guid ThingId { get; } = Guid.Empty;

        public bool IsChanged
        {
            get
            {
                // see if any values have been changed
                foreach (var item in this)
                {
                    if (item.IsChanged)
                    {
                        _IsChanged = true;
                        return true;
                    }
                }
                return _IsChanged;
            }
        }
        private bool _IsChanged = false;

        public void Add(Guid property_id) { this.Add(property_id, ""); }
        public void Add(Guid property_id, string value) { this.Add(property_id, value, false); }
        public void Add(Guid property_id, string value, bool is_existing)
        {
            var prop = new PropertyValue(this.ThingId, property_id, value, is_existing);
            if (!this.Validate(prop)) return;
            prop.OnChanged += this.OnChanged;
            this.Add(prop);
        }

        private PropertyValue GetObject(Guid property_id) { return this.Find(x=>x.PropertyId == property_id); }
        public string GetString(Guid property_id)
        {
            var obj = this.GetObject(property_id);
            if (obj == null) return "";
            if (string.IsNullOrEmpty(obj.Value)) return "";
            return obj.Value;
        }
        public int GetInt (Guid property_id)
        {
            var str = this.GetString(property_id);
            int value = 0;
            int.TryParse(str, out value);
            return value;
        }
        public bool GetBoolean (Guid property_id)
        {
            var str = this.GetString(property_id);
            bool value = false;
            if(!bool.TryParse(str, out value))
            {
                int i = this.GetInt(property_id);
                if(i == 1)
                {
                    value = true;
                }
                else
                {
                    value = false;
                }
            }
            return value;
        }
        public decimal GetDecimal(Guid property_id)
        {
            var str = this.GetString(property_id);
            decimal value = 0;
            decimal.TryParse(str, out value);
            return value;
        }
        public DateTime GetDateTime(Guid property_id)
        {
            var str = this.GetString(property_id);
            DateTime value = DateTime.MinValue;
            DateTime.TryParse(str, out value);
            return value;
        }
        public Guid GetGuid(Guid property_id)
        {
            var str = this.GetString(property_id);
            Guid value = Guid.Empty;
            Guid.TryParse(str, out value);
            return value;
        }

        public void Dispose()
        {
            foreach(var item in this)
            {
                item.OnChanged -= this.OnChanged;
            }
        }

        public bool Exists(Guid property_id) { return this.Exists(x => x.PropertyId == property_id); }

        public bool Refresh()
        {
            // clear this
            this.Clear();

            // build sql
            var sql = new StringBuilder();
            sql.Append("SELECT [PropertyId], [Value] FROM [thing].[Property] WHERE [ThingId] = '");
            sql.Append(this.ThingId);
            sql.Append("'");

            // start reader
            if(!Thingy.DB.StartReader(sql.ToString())) return false;

            // read
            while(Thingy.DB.Reader.Read())
            {
                var prop = Thingy.DB.Reader.GetGuid(0);
                var value = Thingy.DB.Reader.GetString(1);
                this.Add(prop, value, true);
            }

            // disconnect
            Thingy.DB.Disconnect();

            // event
            var handler = this.OnRefreshed;
            handler?.Invoke(this, EventArgs.Empty);

            // default
            return true;
        }

        public void Remove(Guid property_id)
        {
            if (this.Exists(property_id))
            {
                // remove
                var idx = this.FindIndex(x => x.PropertyId == property_id);
                this.RemoveAt(idx);

                // changed
                _IsChanged = true;
                var handler = this.OnChanged;
                handler.Invoke(this, EventArgs.Empty);
            }
        }

        public bool Save()
        {
            // validate
            if (!this.Validate()) return false;

            // sql
            var sql = new StringBuilder();
            foreach(var item in this)
            {
                sql.AppendLine(item.GetSaveString());
            }

            // execute sql
            if(!Thingy.DB.Execute(sql.ToString())) return false;

            // default
            return this.Saved();
        }
        private bool Saved()
        {
            // set change/new flag on each value
            foreach(var item in this)
            {
                item.Saved();
            }

            // event
            var handler = this.OnSaved;
            handler?.Invoke(this, EventArgs.Empty);

            // default
            return true;
        }

        public void SetValue(Guid property_id, string value)
        {
            if (this.Exists(property_id))
            {
                this.GetObject(property_id).Value = value;
            }
            else
            {
                this.Add(property_id, value, false);
            }
        }
        public void SetValue(Guid property_id, int value) { this.SetValue(property_id, value.ToString()); }
        public void SetValue(Guid property_id, bool value) { this.SetValue(property_id, value.ToString()); }
        public void SetValue(Guid property_id, decimal value) { this.SetValue(property_id, value.ToString()); }
        public void SetValue(Guid property_id, DateTime value) { this.SetValue(property_id, value.ToShortDateString() + " " + value.ToShortTimeString()); }
        public void SetValue(Guid property_id, Guid value) { this.SetValue(property_id, value.ToString()); }

        public bool Validate()
        {
            // loop through and validate each
            foreach(var item in this)
            {
                if (!this.Validate(item)) return false;
            }

            // default
            return true;
        }
        public bool Validate(IPropertyValue value)
        {
            return value.Validate();
        }

        public event EventHandler OnChanged;
        public event EventHandler OnRefreshed;
        public event EventHandler OnSaved;

    }

    public interface IPropertyValueList : IDisposable
    {
        Guid ThingId { get; }

        int Count { get; }
        bool IsChanged { get; }

        void Add(Guid property_id, string value);
        void Add(Guid property_id);
        bool Exists(Guid property_id);

        string GetString(Guid property_id);
        int GetInt(Guid property_id);
        bool GetBoolean(Guid property_id);
        decimal GetDecimal(Guid property_id);
        DateTime GetDateTime(Guid property_id);
        Guid GetGuid(Guid property_id);

        void Remove(Guid property_id);

        void SetValue(Guid property_id, string value);
        void SetValue(Guid property_id, int value);
        void SetValue(Guid property_id, bool value);
        void SetValue(Guid property_id, decimal value);
        void SetValue(Guid property_id, DateTime value);
        void SetValue(Guid property_id, Guid value);

        bool Refresh();
        bool Save();
        bool Validate();
        bool Validate(IPropertyValue value);

        event EventHandler OnChanged;
        event EventHandler OnRefreshed;
        event EventHandler OnSaved;

    }

}
