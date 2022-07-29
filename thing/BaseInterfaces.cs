using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace thing
{
    
    public interface IChangeable
    {
        bool IsChanged { get; }

        event EventHandler<PropertyChangeArgs> OnChanged;
    }

    public interface IData : IErrorHandling, IChangeable
    {
        bool IsNew { get; }

        bool RefreshData();
        bool SaveData();
        bool SaveData(bool validate);
        bool ValidateData();

        event EventHandler<ErrorEventArgs> OnValidationFail;
    }

    public interface IErrorHandling
    {
        event EventHandler<ErrorEventArgs> OnError;
    }

    public interface IStatusHandling
    {
        event EventHandler<StatusEventArgs> OnStatus;
    }

}
