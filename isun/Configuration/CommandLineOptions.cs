using CommandLine;

namespace isun.Configuration;

public class CommandLineOptions
{
    [Option("cities")]
    public IEnumerable<string> Cities { get; set; }
}