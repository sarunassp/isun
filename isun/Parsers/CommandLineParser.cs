using MoreLinq;

namespace isun.Parsers;

public interface ICommandLineParser
{
    bool TryParseArguments(ICollection<string> args, string flag, string separator, out IList<string> parsedArgs);
}

public class CommandLineParser : ICommandLineParser
{
    public bool TryParseArguments(ICollection<string> args, string flag, string separator, out IList<string> parsedArgs)
    {
        parsedArgs = new List<string>();
        if (args?.FirstOrDefault()?.Equals(flag, StringComparison.CurrentCultureIgnoreCase) != true ||
            args.Count < 2)
        {
            return false;
        }

        // skip flag and last element (which will be added later)
        foreach (var arg in args.Slice(1, args.Count - 2))
        {
            if (arg.EndsWith(separator) == false)
                return false;

            parsedArgs.Add(arg.Remove(arg.LastIndexOf(separator, StringComparison.CurrentCultureIgnoreCase)));
        }
        parsedArgs.Add(args.Last());

        return true;
    }
}