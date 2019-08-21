using System.Windows;
using System.Windows.Input;

namespace AlarmClock.Commands
{
    class QuitCommand : CommandBase<QuitCommand>
    {
        private bool isClosed = false;
        public override void Execute(object parameter)
        {
            isClosed = true;
            GetTaskbarWindow(parameter).Close();
            CommandManager.InvalidateRequerySuggested();
        }

        public bool IsClosed()
        {
            return isClosed;
        }

        public override bool CanExecute(object parameter)
        {
            Window win = GetTaskbarWindow(parameter);
            return win != null;
        }
    }
}
