using System.Collections.Concurrent;
using TUnit.Assertions;
using TUnit.Assertions.Extensions;
using TUnit.Core;

namespace InputManager.Tests;

public class InputManagerTests
{
    private static MockConsole _console = new();
    private static InputActionManager _inputManager;

    private static ConcurrentDictionary<ConsoleKey, int> _results = new();
    private static CancellationTokenSource _cts = new();

    [Before(HookType.Class)]
    public static void SetupInputManager()
    {
        _inputManager = new(_console);
        _inputManager
            .RegisterAction(keyInfo => _results.AddOrUpdate(keyInfo.Key, 1, (_, value) => value + 1), ConsoleKey.A, ConsoleKey.B, ConsoleKey.C)
            .RegisterAction(async (keyInfo, _) =>
            {
                _results.AddOrUpdate(keyInfo.Key, 1, (_, value) => value + 1);
            }, ConsoleKey.C, ConsoleKey.D, ConsoleKey.E, ConsoleKey.F)
            .RegisterAction(async (keyInfo, token) =>
            {
                await Task.Delay(5000, token);
                throw new Exception();
            }, ConsoleKey.G)
            .RegisterAction(_ => _cts.Cancel(), ConsoleKey.Escape);

    }

    [After(HookType.Class)]
    public static void Dispose()
    {
        _console.Dispose();
    }

    [After(HookType.Test)]
    public void ResetResults()
    {
        _results.Clear();
    }

    [Test]
    public async Task InputManagerShouldExecute()
    {
        _console.SetKeySequence([ConsoleKey.A, ConsoleKey.C, ConsoleKey.D, ConsoleKey.E, ConsoleKey.A, ConsoleKey.Escape, ConsoleKey.A]);
        await _inputManager.StartListener(_cts.Token);
        var expectedDictionary = new Dictionary<ConsoleKey, int>()
        {
            [ConsoleKey.A] = 2,
            [ConsoleKey.C] = 2,
            [ConsoleKey.D] = 1,
            [ConsoleKey.E] = 1,
        };
        await Assert.That(_results.ToDictionary()).IsEquivalentTo(expectedDictionary);
    }

    [Test]
    [NotInParallel]
    public async Task InputManagerShouldCancel()
    {
        _console.SetKeySequence([ConsoleKey.G]);
        _cts.CancelAfter(1000);
        await Assert.That(_inputManager.StartListener(_cts.Token)).Throws<TaskCanceledException>();
    }
}
