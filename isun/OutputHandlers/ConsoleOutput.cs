namespace isun.OutputHandlers;

public interface IOutputHandler
{
    void WriteLine(string message);
}

public class ConsoleOutputHandler : IOutputHandler
{
    public void WriteLine(string message)
    {
        Console.WriteLine(message);
    }
}