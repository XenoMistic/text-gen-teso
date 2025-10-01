using System.Windows.Input;

namespace UndauntedPledges.Implementations;

public class DelegateCommand: ICommand
{
    private readonly Action<object?> _action;
    private readonly Func<object?, bool>? _canExecute;

    public event EventHandler? CanExecuteChanged;

    public DelegateCommand(Action<object?> action, Func<object?, bool>? canExecute = null)
    {
        ArgumentNullException.ThrowIfNull(action);
        _action = action;
        _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter)
    {
        if (_canExecute is null)
        {
            return true;
        }

        return _canExecute(parameter);
    }

    public void Execute(object? parameter)
    {
        _action(parameter);
    }

    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}