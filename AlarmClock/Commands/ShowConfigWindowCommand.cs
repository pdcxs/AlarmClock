using System.Windows;
using System.Windows.Input;

namespace AlarmClock.Commands
{
    public class ShowConfigWindowCommand : CommandBase<ShowConfigWindowCommand>
    {
        public override void Execute(object parameter)
        {
            GetTaskbarWindow(parameter).Show();
            GetTaskbarWindow(parameter).WindowState = WindowState.Normal;
            CommandManager.InvalidateRequerySuggested();
        }


        public override bool CanExecute(object parameter)
        {
            Window win = GetTaskbarWindow(parameter);
            return win != null && !win.IsVisible;
        }
    }
}
