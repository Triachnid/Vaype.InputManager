namespace InputManager.Tests;

public class MockConsole : IConsole, IDisposable
{
    private IEnumerator<ConsoleKeyInfo> _testKeyEnumerator = null;

    public void SetKeySequence(IEnumerable<ConsoleKey> keys)
    {
        _testKeyEnumerator?.Dispose();
        _testKeyEnumerator = keys.Select(x => new ConsoleKeyInfo(default, x, false, false, false)).GetEnumerator();
    }

    public void Dispose()
    {
        _testKeyEnumerator?.Dispose();
    }

    public ConsoleKeyInfo ReadKey(bool intercept)
    {
        _testKeyEnumerator?.MoveNext();
        return _testKeyEnumerator?.Current??default;
    }
}
