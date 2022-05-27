using isun.Parsers;

namespace isun.UnitTests.Parsers;

public class CommandLineParserTests
{
    [Test]
    public void TryParserArguments_ArgsSeparatedWithCorrectSeparator_OutputsCorrectParsedArgsReturnsTrue()
    {
        // Arrange
        // '--someflag arg1, arg2, arg3' is parsed to this array and passed to Program.cs
        var args = new[] { "--someflag", "arg1,", "arg2,", "arg3" };

        var sut = new CommandLineParser();

        // Act
        var isSuccess = sut.TryParseArguments(args, "--someflag", ",", out var parsedArgs);

        // Assert
        Assert.True(isSuccess);
        CollectionAssert.AreEqual(new[] { "arg1", "arg2", "arg3" }, parsedArgs);
    }

    [Theory]
    [TestCaseSource(nameof(InvalidArgsCases))]
    public void TryParserArguments_InvalidArgs_Cases(string[] args)
    {
        // Arrange
        var flag = DefaultFixture.Fixture.Create<string>();
        var separator = DefaultFixture.Fixture.Create<string>();

        var sut = new CommandLineParser();

        // Act
        var isSuccess = sut.TryParseArguments(args, flag, separator, out var parsedArgs);

        // Assert
        Assert.False(isSuccess);
    }

    [Test]
    public void TryParserArguments_FirstArgDoesNotMatchFlag_ReturnsFalse()
    {
        // Arrange
        var args = DefaultFixture.Fixture.Create<string[]>();
        var flag = DefaultFixture.Fixture.Create<string>();
        var separator = DefaultFixture.Fixture.Create<string>();

        var sut = new CommandLineParser();

        // Act
        var isSuccess = sut.TryParseArguments(args, flag, separator, out var parsedArgs);

        // Assert
        Assert.False(isSuccess);
    }

    [Test]
    public void TryParserArguments_OnlyFlagProvided_ReturnsFalse()
    {
        // Arrange
        var flag = DefaultFixture.Fixture.Create<string>();
        var args = new[] { flag };
        var separator = DefaultFixture.Fixture.Create<string>();

        var sut = new CommandLineParser();

        // Act
        var isSuccess = sut.TryParseArguments(args, flag, separator, out var parsedArgs);

        // Assert
        Assert.False(isSuccess);
    }

    [Test]
    public void TryParserArguments_ArgsSeparatedWithWrongSeparator_ReturnsFalse()
    {
        // Arrange
        var args = DefaultFixture.Fixture.Create<string[]>();
        var flag = args.First();
        var separator = DefaultFixture.Fixture.Create<string>();

        var sut = new CommandLineParser();

        // Act
        var isSuccess = sut.TryParseArguments(args, flag, separator, out var parsedArgs);

        // Assert
        Assert.False(isSuccess);
    }

    private static IEnumerable<string[]> InvalidArgsCases
    {
        get
        {
            yield return new string[] { "onlyflag" };
            yield return new string[] { "" };
            yield return new string[] { };
            yield return (string[])null;
        }
    }
}