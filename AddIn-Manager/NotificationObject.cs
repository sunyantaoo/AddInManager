using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddIn_Manager
{
    public abstract class NotificationObject : INotifyPropertyChanged
    {
        private event PropertyChangedEventHandler propertyChanged;
        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add { propertyChanged += value; }
            remove { propertyChanged -= value; }
        }

        protected void RaisePropertyChanged(string propertyName)
        {
            propertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
