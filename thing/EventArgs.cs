using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace thing
{
    
    public class MessageEventArgs : EventArgs
    {

        protected MessageEventArgs(string message)
        {
            this.Message = message;
        }

        public string Message { get; set; } = "";

        public static new MessageEventArgs Empty { get; } = new MessageEventArgs("");

    }

    public class StatusEventArgs : MessageEventArgs
    {
        public StatusEventArgs(string message) : base(message) { }
        public StatusEventArgs(string message, bool continue_processing) : base(message)
        {
            this.ContinueProcessing = continue_processing;
            if (!continue_processing) this.TextColor = Color.Red;
        }
        public StatusEventArgs(string message, bool continue_processing, Color text_color) : base(message)
        {
            this.ContinueProcessing = continue_processing;
            this.TextColor = text_color;
        }

        public bool ContinueProcessing { get; set; } = true;
        public Color TextColor { get; set; } = Color.Black;

    }

    public class ErrorEventArgs : MessageEventArgs
    {
        public ErrorEventArgs(string message) : base(message) { }
        public ErrorEventArgs(Exception e) : base(e.Message) { }

        public bool ReturnValue { get; } = false;
    }

    public class PropertyChangeArgs : EventArgs
    {
        public PropertyChangeArgs(string prop_name, string old_value, string new_value)
        {
            this.PropertyName = prop_name;
            this.OldValue = old_value;
            this.NewValue = new_value;
        }
        public PropertyChangeArgs(string prop_name, int old_value, int new_value)
        {
            this.PropertyName = prop_name;
            this.OldValue = old_value.ToString();
            this.NewValue = new_value.ToString();
        }
        public PropertyChangeArgs(string prop_name, DateTime old_value, DateTime new_value)
        {
            this.PropertyName = prop_name;
            this.OldValue = old_value.ToShortDateString() + " " + old_value.ToShortTimeString();
            this.NewValue = new_value.ToShortDateString() + " " + new_value.ToShortTimeString();
        }
        public PropertyChangeArgs(string prop_name, decimal old_value, decimal new_value)
        {
            this.PropertyName = prop_name;
            this.OldValue = old_value.ToString();
            this.NewValue = new_value.ToString();
        }
        public PropertyChangeArgs(string prop_name, bool old_value, bool new_value)
        {
            this.PropertyName = prop_name;
            this.OldValue = old_value.ToString();
            this.NewValue = new_value.ToString();
        }
        public PropertyChangeArgs(string prop_name, Guid old_value, Guid new_value)
        {
            this.PropertyName = prop_name;
            this.OldValue = old_value.ToString();
            this.NewValue = new_value.ToString();
        }

        public string PropertyName { get; }
        public string OldValue { get; }
        public string NewValue { get; }
    }

    public class SqlEventArgs : EventArgs
    {
        public SqlEventArgs(string sql)
        {
            this.Sql = sql;
        }

        public string Sql { get; }
    }

    public class LoggableEventArgs : EventArgs
    {
        public LoggableEventArgs(string log_data, int log_level)
        {
            this.LogData = log_data;
            this.LogLevel = log_level;
        }
        public LoggableEventArgs(string log_data)
        {
            this.LogData = log_data;
        }

        public string LogData { get; }
        public int LogLevel { get; } = 0;
    }

}
