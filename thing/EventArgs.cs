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
        public PropertyChangeArgs(string old_value, string new_value)
        {
            OldValue = old_value;
            NewValue = new_value;
        }
        public PropertyChangeArgs(int old_value, int new_value)
        {
            this.OldValue = old_value.ToString();
            this.NewValue = new_value.ToString();
        }
        public PropertyChangeArgs(DateTime old_value, DateTime new_value)
        {
            this.OldValue = old_value.ToShortDateString() + " " + old_value.ToShortTimeString();
            this.NewValue = new_value.ToShortDateString() + " " + new_value.ToShortTimeString();
        }
        public PropertyChangeArgs(decimal old_value, decimal new_value)
        {
            this.OldValue = old_value.ToString();
            this.NewValue = new_value.ToString();
        }
        public PropertyChangeArgs(bool old_value, bool new_value)
        {
            this.OldValue = old_value.ToString();
            this.NewValue = new_value.ToString();
        }
        public PropertyChangeArgs(Guid old_value, Guid new_value)
        {
            this.OldValue = old_value.ToString();
            this.NewValue = new_value.ToString();
        }

        public string OldValue { get; }
        public string NewValue { get; }
    }

}
