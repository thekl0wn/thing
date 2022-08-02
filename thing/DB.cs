using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace thing
{

    internal class ThingDatabase : IThingDatabase
    {

        internal DbReader Reader { get; private set; }

        public string DatabaseName { get; set; } = Properties.Settings.Default.DatabaseName;
        public string ServerName { get; set; } = Properties.Settings.Default.ServerName;
        public bool IntegratedSecurity { get; set; } = Properties.Settings.Default.IntegratedSecurity;

        private SqlCommand _Command;
        private SqlConnection _Connection;
        internal string ConnectionString
        {
            get
            {
                var b = new SqlConnectionStringBuilder();
                b.DataSource = this.ServerName;
                b.InitialCatalog = this.DatabaseName;
                b.IntegratedSecurity = this.IntegratedSecurity;
                return b.ConnectionString;
            }
        }

        private bool Connect()
        {
            // disconnect
            this.Disconnect();

            // create
            if (_Connection == null) _Connection = new SqlConnection(this.ConnectionString);

            // connect
            try
            {
                _Connection.Open();
            }
            catch(Exception e)
            {
                return this.Error(e);
            }

            // return based on connection state
            if(_Connection.State == System.Data.ConnectionState.Open)
            {
                // event
                var handler = this.OnConnected;
                handler?.Invoke(this, EventArgs.Empty);

                // return success
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool CreateCommand(string sql)
        {
            // validate sql
            if (!this.ValidateSql(sql)) return false;

            // connect
            if (!this.Connect()) return false;

            // create command from connection & set it up
            _Command = _Connection.CreateCommand();
            _Command.CommandText = sql;

            // default
            return true;
        }

        internal void Disconnect()
        {
            // command
            if(_Command != null)
            {
                _Command.Dispose();
            }

            // reader
            if(this.Reader != null)
            {
                this.Reader.OnReaderRead -= this.OnReaderRead;
                this.Reader.Dispose();
                this.Reader = null;
            }

            // connection
            if(_Connection != null)
            {
                // close it
                _Connection.Close();

                // check state
                if(_Connection.State != System.Data.ConnectionState.Open)
                {
                    // event
                    var handler = this.OnDisconnected;
                    handler?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        internal bool Error(Exception e)
        {
            this.Disconnect();
            var args = new ErrorEventArgs(e);
            var handler = this.OnError;
            handler?.Invoke(this, args);
            return false;
        }
        internal bool Error(string error)
        {
            this.Disconnect();

            // event
            var args = new ErrorEventArgs(error);
            var handler = this.OnError;
            handler?.Invoke(this, args);

            // always return false
            return false;
        }

        internal bool Execute(string sql)
        {
            // create the command
            if (!this.CreateCommand(sql)) return false;

            try
            {
                // execute as non-query
                _Command.ExecuteNonQuery();
            }
            catch(Exception e)
            {
                return this.Error(e);
            }

            // disconnect
            this.Disconnect();

            // event
            var args = new SqlEventArgs(sql);
            var handler = this.OnExecute;
            handler?.Invoke(this, args);

            // default
            return true;
        }
        internal bool Execute(Queue<string> queue)
        {
            foreach(string sql in queue)
            {
                if (!this.Execute(sql)) return false;
            }

            return true;
        }

        internal bool Scalar(string sql, out object value)
        {
            value = null;

            // create the command
            if (!this.CreateCommand(sql)) return false;

            // execute as scalar
            try
            {
                value = _Command.ExecuteScalar();
            }
            catch(Exception e)
            {
                return this.Error(e);
            }

            // disconnect
            this.Disconnect();

            // event
            var args = new SqlEventArgs(sql);
            var handler = this.OnScalar;
            handler?.Invoke(this, args);

            return true;
        }
        internal bool Scalar(string sql, out string value)
        {
            value = "";
            object obj = null;
            if (!this.Scalar(sql, out obj)) return false;
            value = obj.ToString();
            return true;
        }
        internal bool Scalar(string sql, out Guid value)
        {
            value = Guid.Empty;
            string str = "";
            if (!this.Scalar(sql, out str)) return false;
            if(!Guid.TryParse(str, out value)) return false;

            // default
            return true;
        }

        internal bool StartReader(string sql)
        {
            // create the command
            if (!this.CreateCommand(sql))
            {
                this.Disconnect();
                return false;
            }

            // execute as reader
            try
            {
                this.Reader = new DbReader(_Command.ExecuteReader());
                this.Reader.OnReaderRead += this.OnReaderRead;
            }
            catch(Exception e)
            {
                return this.Error(e);
            }

            // calling object's responsibility to disconnect

            // event
            var args = new SqlEventArgs(sql);
            var handler = this.OnReaderStarted;
            handler?.Invoke(this, args);

            // default
            return true;
        }

        internal void Status() { this.Status("Ready"); }
        internal void Status(string status)
        {
            var args = new StatusEventArgs(status);
            var handler = this.OnStatus;
            handler?.Invoke(this, args);
        }

        public bool TestConnection()
        {
            // try to connect
            var connected = this.Connect();

            // disconnect
            this.Disconnect();

            // default
            if (connected)
            {
                // event
                var handler = this.OnTestSuccess;
                handler?.Invoke(this, EventArgs.Empty);
                
                // return
                return true;
            }
            else
            {
                // event
                var args = new ErrorEventArgs("Connection test failed.");
                var handler = this.OnTestFail;
                handler?.Invoke(this, args);
                return false;
            }
        }

        private bool ValidateSql(string sql)
        {
            // blank?
            if (string.IsNullOrEmpty(sql)) return false;

            // default
            return true;
        }

        internal class DbReader : IDisposable
        {

            internal DbReader(SqlDataReader reader)
            {
                this.SqlReader = reader;
            }

            private SqlDataReader SqlReader { get; set; }

            public void Dispose()
            {
                if(SqlReader != null)
                {
                    SqlReader.Close();
                    SqlReader = null;
                }
            }

            internal bool HasRows { get { return SqlReader.HasRows; } }

            internal string GetString(int column) { return SqlReader.GetString(column); }
            internal Guid GetGuid(int column) { return SqlReader.GetGuid(column); }
            internal int GetInt(int column) { return SqlReader.GetInt32(column); }
            internal bool GetBoolean(int column)
            {
                var value = false;
                var str = this.GetString(column);
                if (!bool.TryParse(str, out value)) return false;
                return value;
            }

            internal bool Read() 
            {
                var read = SqlReader.Read();
                if (read)
                {
                    var handler = this.OnReaderRead;
                    handler?.Invoke(this, EventArgs.Empty);
                }
                return read;
            }

            internal event EventHandler OnReaderRead;
        }

        public event EventHandler OnConnected;
        public event EventHandler OnDisconnected;
        public event EventHandler<ErrorEventArgs> OnError;
        public event EventHandler<StatusEventArgs> OnStatus;
        public event EventHandler<SqlEventArgs> OnExecute;
        public event EventHandler<SqlEventArgs> OnScalar;
        public event EventHandler OnTestSuccess;
        public event EventHandler OnTestFail;
        public event EventHandler<SqlEventArgs> OnReaderStarted;
        public event EventHandler OnReaderRead;

    }

    public interface IThingDatabase
    {
        string DatabaseName { get; set; }
        string ServerName { get; set; }
        bool IntegratedSecurity { get; set; }

        bool TestConnection();

        event EventHandler OnConnected;
        event EventHandler OnDisconnected;
        event EventHandler<ErrorEventArgs> OnError;
        event EventHandler<StatusEventArgs> OnStatus;
        event EventHandler<SqlEventArgs> OnExecute;
        event EventHandler<SqlEventArgs> OnScalar;
        event EventHandler OnTestSuccess;
        event EventHandler OnTestFail;
        event EventHandler<SqlEventArgs> OnReaderStarted;
        event EventHandler OnReaderRead;
    }

}
