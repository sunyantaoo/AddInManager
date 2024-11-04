using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AddIn_Manager
{
    public class DelegateCommand : ICommand
    {
        public DelegateCommand(Action cmdMethod)
        {
            this._commandMethod = cmdMethod;
        }

        public DelegateCommand(Action cmdMethod,Func<bool> cmdCanExecute)
        {
            this._commandMethod = cmdMethod;
            this._commandCanExecute = cmdCanExecute;
        }

        private EventHandler canExecuteChanged;
        event EventHandler ICommand.CanExecuteChanged
        {
            add { canExecuteChanged += value; }
            remove { canExecuteChanged -= value; }
        }

        private Action _commandMethod;
        private Func<bool> _commandCanExecute;

        bool ICommand.CanExecute(object parameter)
        {
            if (_commandCanExecute == null) return true;
            return _commandCanExecute.Invoke();
        }

        public void Execute() 
        {
            _commandMethod.Invoke();
        }

        void ICommand.Execute(object parameter)
        {
            _commandMethod.Invoke();
        }
        /// <summary>
        /// 触发命令可执行方法
        /// </summary>
        public void RaiseCanExecuteChanged() 
        {
            canExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public class DelegateCommand<T> : ICommand
    {
        public DelegateCommand(Action<T> cmdMethod)
        {
            this._commandMethod = cmdMethod;
        }

        public DelegateCommand(Action<T> cmdMethod, Predicate<T> cmdCanExecute)
        {
            this._commandMethod = cmdMethod;
            this._commandCanExecute = cmdCanExecute;
        }

        private EventHandler canExecuteChanged;
        event EventHandler ICommand.CanExecuteChanged
        {
            add { canExecuteChanged += value; }
            remove { canExecuteChanged -= value; }
        }

        private Action<T> _commandMethod;
        private Predicate<T> _commandCanExecute;

        bool ICommand.CanExecute(object parameter)
        {
            if (_commandCanExecute == null) return true;
            return _commandCanExecute.Invoke((T)parameter);
        }

        void ICommand.Execute(object parameter)
        {
            _commandMethod.Invoke((T)parameter);
        }
        /// <summary>
        /// 触发命令可执行方法
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            canExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
