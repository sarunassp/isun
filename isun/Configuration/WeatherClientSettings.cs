namespace isun.Configuration;

public class WeatherClientSettings
{
    public const string Section = "WeatherClientSettings";
    
    public string Username { get; private set; }

    public string Password { get; private set; }

    public Uri BaseUri { get; private set; }
}