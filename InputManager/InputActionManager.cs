using System.Collections.Concurrent;

namespace InputManager;

public class InputActionManager(IConsole console)
{
    public static InputActionManager Create()
        => new(new ConsoleWrapper());

    private readonly ConcurrentDictionary<ConsoleKey, Func<ConsoleKeyInfo, CancellationToken, Task>> _actions = new();

    private readonly IConsole _console = console;

    public InputActionManager RegisterAction(Action<ConsoleKeyInfo> action, params ConsoleKey[] keys)
        => RegisterAction((keyInfo, _) =>
        {
            action(keyInfo);
            return Task.CompletedTask;
        }, keys);

    public InputActionManager RegisterAction(Func<ConsoleKeyInfo, CancellationToken, Task> action, params IEnumerable<ConsoleKey> keys)
    {
        foreach(var key in keys)
        {
            if (_actions.ContainsKey(key))
                _actions[key] += action;
            else
                _actions[key] = action;
        }

        return this;
    }

    public Task StartListener(CancellationToken cancellationToken = default)
        => Task.Run(() =>
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var keyInfo = _console.ReadKey(true);
                if (!_actions.ContainsKey(keyInfo.Key))
                {
                    continue;
                }
                var keyActions = _actions[keyInfo.Key];
                if(keyActions != default)
                {
                    foreach(var action in keyActions.GetInvocationList())
                    {
                        action.DynamicInvoke(keyInfo, cancellationToken);
                    }
                }
            }
        }, cancellationToken);
}
