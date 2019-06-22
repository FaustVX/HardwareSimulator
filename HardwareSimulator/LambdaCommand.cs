using System;
using System.Windows.Input;

namespace HardwareSimulator
{
    public sealed class LambdaCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private readonly Func<bool> _canExecute;
        private readonly Action _execute;

        public bool LastExecute { get; private set; }

        public LambdaCommand(Func<bool> canExecute, Action execute)
        {
            _canExecute = canExecute;
            _execute = execute;
        }

        public bool CanExecute(object parameter)
            => LastExecute = _canExecute();

        public void Execute(object parameter)
            => _execute();

        public void OnCanExecuteChanged()
            => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
