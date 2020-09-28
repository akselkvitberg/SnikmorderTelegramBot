using System;
using System.Windows.Input;

namespace Snikmorder.DesktopClient
{
    public class RelayCommand : ICommand
    {
        readonly Action<object> execute;
        readonly Predicate<object> canExecute;

        /// <summary>
        /// Occurs when [internal can execute changed].
        /// </summary>
        event EventHandler internalCanExecuteChanged;

        /// <summary>
        /// Creates a new command that can always execute.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        public RelayCommand(Action<object> execute) : this(execute, null) { }

        /// <summary>
        /// Creates a new command.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <param name="canExecute">The execution status logic.</param>
        public RelayCommand(Action<object> execute, Predicate<object> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");

            this.execute = execute;
            this.canExecute = canExecute;
        }

        /// <summary>
        /// Code to determine if the command is executable.
        /// </summary>
        public bool CanExecute(object parameter)
        {
            return canExecute == null || canExecute(parameter);
        }

        /// <summary>
        /// Occurs when changes occur that affect whether or not the command should execute.
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add
            {
                internalCanExecuteChanged += value;
                CommandManager.RequerySuggested += value;
                CommandManager.InvalidateRequerySuggested();
            }
            remove
            {
                internalCanExecuteChanged -= value;
                CommandManager.RequerySuggested -= value;
            }
        }

        /// <summary>
        /// Code to run when actually executing the command.
        /// </summary>
        public void Execute(object parameter)
        {
            execute(parameter);
        }

        /// <summary>
        /// Reevaluates the can execute.
        /// </summary>
        public void ReevaluateCanExecute()
        {
            if (internalCanExecuteChanged != null)
                internalCanExecuteChanged(this, System.EventArgs.Empty);
        }
    }
}