namespace InputManager;

public interface IConsole
{
    ConsoleKeyInfo ReadKey(bool intercept);
}