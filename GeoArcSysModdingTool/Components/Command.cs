using System;
using System.Windows.Input;

namespace GeoArcSysModdingTool.Components
{
    public class Command<T> : ICommand
    {
        private readonly Action<T> action;
        private readonly Func<T, bool> canExecute;

        public Command(Action<T> action, Func<T, bool> canExecute = null)
        {
            this.action = action;
            this.canExecute = canExecute;
        }

        public Command(Action action, Func<bool> canExecute = null)
        {
            this.action = x => action();
            if (canExecute != null)
                this.canExecute = x => canExecute();
        }

        // ICommand
        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object parameter)
        {
            return canExecute == null || canExecute((T) parameter);
        }

        public void Execute(object parameter)
        {
            action((T) parameter);
        }
    }

    public class Command : Command<object>
    {
        public Command(Action action, Func<bool> canExecute = null) : base(action, canExecute)
        {
        }
    }
}