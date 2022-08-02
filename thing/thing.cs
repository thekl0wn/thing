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

        internal Thing(Guid id)
        {
            this.Status("Loading thing [" + id.ToString().ToUpper() + "]");
            this.Id = id;
            this.IsNew = false;
            _Properties = new PropertyValueList(this.Id);
            this.Refresh();
        }
            
        internal Thing(Guid type_id, Guid repo_id)
        {
            this.Id = Guid.NewGuid();
            this.Status("Creating new thing. ID = [" + this.Id.ToString().ToUpper() + "]");
            this.TypeId = type_id;
            this.RepositoryId = repo_id;
            this.IsNew = true;
            this.StatusId = StatusThing.ID_ACTIVE;
            _Properties = new PropertyValueList(this.Id);
            
            // subscribe
            _Properties.OnChanged += this.OnChanged;
        }

        public Guid Id { get; } = Guid.Empty;
        public Guid StatusId { get; set; } = Guid.Empty;
        private Guid _OriginalStatus = Guid.Empty;
        public Guid TypeId { get; private set; } = Guid.Empty;
        private Guid _OriginalType = Guid.Empty;
        public bool IsNew { get; private set; } = false;
        public Guid RepositoryId { get; private set; } = Guid.Empty;
        private Guid _OriginalRepository = Guid.Empty;

        public string Name
        {
            get { return _Properties.GetString(PropertyThing.ID_NAME); }
            set { _Properties.SetValue(PropertyThing.ID_NAME, value); }
        }
        public string Code
        {
            get { return _Properties.GetString(PropertyThing.ID_CODE); }
            set { _Properties.SetValue(PropertyThing.ID_CODE, value); }
        }
        public string Description
        {
            get { return _Properties.GetString(PropertyThing.ID_DESCRIPTION); }
            set { _Properties.SetValue(PropertyThing.ID_DESCRIPTION, value); }
        }

        public IPropertyValueList Properties { get { return _Properties; } }
        private PropertyValueList _Properties;

        public void MoveRepository(Guid new_repo)
        {
            // check repo
            if (this.RepositoryId == new_repo) return;
            if (!Thingy.Things.Repositories.Exists(x => x.Id == new_repo)) return;

            // change repo
            this.RepositoryId = new_repo;
            if (!this.Save()) return;
        }

        public void Dispose()
        {
            // properties
            _Properties.OnChanged -= this.OnChanged;
            _Properties.Dispose();
            _Properties = null;

        }

        protected bool Error(Exception e) { return this.Error(e.Message); }
        protected bool Error(string error)
        {
            // event
            var args = new ErrorEventArgs(error);
            var handler = this.OnError;
            handler?.Invoke(this, args);

            // always return false
            return false;
        }

        protected virtual Queue<string> GetInserts()
        {
            var queue = new Queue<string>();
            var sql = new StringBuilder();
            sql.Append("INSERT [thing].[Master] ( [Id], [TypeId], [StatusId], [RepositoryId] ) SELECT '");
            sql.Append(this.Id);
            sql.Append("', '");
            sql.Append(this.TypeId);
            sql.Append("', '");
            sql.Append(this.StatusId);
            sql.Append("', '");
            sql.Append(this.RepositoryId);
            sql.Append("'");
            queue.Enqueue(sql.ToString());
            return queue;
        }
        protected virtual Queue<string> GetUpdates()
        {
            var queue = new Queue<string>();
            if (this.StatusId != _OriginalStatus || this.TypeId != _OriginalType || this.RepositoryId != _OriginalRepository)
            {
                var sql = new StringBuilder();
                sql.Append("UPDATE [thing].[Master] SET [TypeId] = '");
                sql.Append(this.TypeId);
                sql.Append("', [StatusId] = '");
                sql.Append(this.StatusId);
                sql.Append("', [RepositoryId] = '");
                sql.Append(this.RepositoryId);
                sql.Append("' ");
                sql.Append(this.GetWhereClause());
                queue.Enqueue(sql.ToString());
            }
            return queue;
        }
        protected string GetWhereClause()
        {
            // sql
            var sql = new StringBuilder();
            sql.Append(" WHERE [Id] = '");
            sql.Append(this.Id);
            sql.Append("'");

            // return
            return sql.ToString();
        }

        public bool Refresh()
        {
            // properties
            if (!_Properties.Refresh()) return false;

            // data from thing.master
            var sql = new StringBuilder();
            sql.Append("SELECT [TypeId], [StatusId], [RepositoryId] FROM [thing].[Master] ");
            sql.Append(this.GetWhereClause());

            // start reader
            if(!Thingy.DB.StartReader(sql.ToString())) return false;

            // read first line
            if (Thingy.DB.Reader.Read())
            {
                this.TypeId = Thingy.DB.Reader.GetGuid(0);
                this.StatusId = Thingy.DB.Reader.GetGuid(1);
                this.RepositoryId = Thingy.DB.Reader.GetGuid(2);
            }
            else
            {
                // error
                Thingy.DB.Disconnect();
                return false;
            }

            // set original values
            _OriginalStatus = this.StatusId;
            _OriginalType = this.TypeId;
            _OriginalRepository = this.RepositoryId;

            // disconnect
            Thingy.DB.Disconnect();

            // event
            var handler = this.OnRefreshed;
            handler?.Invoke(this, EventArgs.Empty);

            // default
            return true;
        }

        public bool Save()
        {
            // validate
            if (!this.Validate()) return false;

            // save to master
            this.Status("Saving...");
            var queue = new Queue<string>();
            if (this.IsNew)
            {
                queue = this.GetInserts();
            }
            else
            {
                queue = this.GetUpdates();
            }

            // save master
            if (!Thingy.DB.Execute(queue)) return false;

            // save
            if (!_Properties.Save()) return false;

            // default
            return this.Saved();
        }
        private bool Saved()
        {
            // new?
            if (this.IsNew) this.IsNew = false;

            // originals
            _OriginalStatus = this.StatusId;
            _OriginalType = this.TypeId;
            _OriginalRepository = this.RepositoryId;

            // event
            this.Status("Saved");
            var handler = this.OnSaved;
            handler?.Invoke(this, EventArgs.Empty);

            // default
            this.Status();
            return true;
        }

        protected void Status() { this.Status("Ready"); }
        protected void Status(string status)
        {
            // event
            var args = new StatusEventArgs(status);
            var handler = this.OnStatus;
            handler?.Invoke(this, args);
        }

        public bool Validate()
        {
            // status
            this.Status("Validating...");

            // properties
            if (!_Properties.Validate()) return false;

            // default
            this.Status();
            return true;
        }

        public static Guid ID_NULL { get { return Guid.Empty; } }

        public event EventHandler OnChanged;
        public event EventHandler<ErrorEventArgs> OnError;
        public event EventHandler OnRefreshed;
        public event EventHandler OnSaved;
        public event EventHandler<StatusEventArgs> OnStatus;

        public static Guid GetRepositoryId(Guid thing_id)
        {
            var repo_id = Guid.Empty;
            var sql = new StringBuilder();
            sql.Append("SELECT [RepositoryId] FROM [thing].[Master] WHERE [Id] = '");
            sql.Append(thing_id);
            sql.Append("'");
            if (!Thingy.DB.Scalar(sql.ToString(), out repo_id)) repo_id = Guid.Empty;
            return repo_id;
        }
        public static Guid GetStatusId(Guid thing_id)
        {
            var status_id = Guid.Empty;
            var sql = new StringBuilder();
            sql.Append("SELECT [StatusId] FROM [thing].[Master] WHERE [Id] = '");
            sql.Append(thing_id);
            sql.Append("'");
            if (!Thingy.DB.Scalar(sql.ToString(), out status_id)) status_id = Guid.Empty;
            return status_id;
        }
        public static Guid GetTypeId(Guid thing_id)
        {
            var type_id = Guid.Empty;
            var sql = new StringBuilder();
            sql.Append("SELECT [TypeId] FROM [thing].[Master] WHERE [Id] = '");
            sql.Append(thing_id);
            sql.Append("'");
            if (!Thingy.DB.Scalar(sql.ToString(), out type_id)) type_id = Guid.Empty;
            return type_id;
        }
        public static Guid GetKnownId(string type_code, string thing_code)
        {
            var sql = new StringBuilder();
            var id = Guid.Empty;
            sql.Append("SELECT [Id] FROM [thing].[Known] WHERE [TypeCode] = '");
            sql.Append(type_code);
            sql.Append("' AND [ThingCode] = '");
            sql.Append(thing_code);
            sql.Append("'");
            if(!Thingy.DB.Scalar(sql.ToString(), out id)) id = Guid.Empty;
            return id;
        }

    }

    public interface IThing : IDisposable
    {
        Guid Id { get; }
        bool IsNew { get; }
        Guid TypeId { get; }
        Guid StatusId { get; set; }
        string Name { get; set; }
        string Code { get; set; }
        string Description { get; set; }

        IPropertyValueList Properties { get; }

        void MoveRepository(Guid new_repo);

        bool Refresh();

        event EventHandler OnChanged;
        event EventHandler<ErrorEventArgs> OnError;
        event EventHandler OnRefreshed;
        event EventHandler OnSaved;
        event EventHandler<StatusEventArgs> OnStatus;
    }

}
