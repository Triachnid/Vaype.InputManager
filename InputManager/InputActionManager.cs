using System.Collections.Concurrent;

namespace InputManager;

public class InputActionManager
{
    private readonly ConcurrentDictionary<ConsoleKey, Action<ConsoleKeyInfo>> _actions = new();

    public InputActionManager RegisterAction(Action<ConsoleKeyInfo> action, params ConsoleKey[] keys)
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
            // Is this check needed?
            while (!cancellationToken.IsCancellationRequested)
            {
                var keyInfo = Console.ReadKey(true);
                var keyActions = _actions[keyInfo.Key];
                if(keyActions != default)
                {
                    var tasks = new List<Task>();
                    foreach(var action in keyActions.GetInvocationList().Cast<Action<ConsoleKeyInfo>>()) //  Add specific delegate type to prevent cast
                    {
                        tasks.Add(Task.Run(() => 
                            action.Invoke(keyInfo), cancellationToken));
                    }
                    // Task.WaitAll(tasks.ToArray())
                }
            }
        }, cancellationToken);
}
