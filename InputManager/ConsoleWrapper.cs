namespace InputManager;

internal class ConsoleWrapper : IConsole
{
    public ConsoleKeyInfo ReadKey(bool intercept)
        => Console.ReadKey(intercept);
}