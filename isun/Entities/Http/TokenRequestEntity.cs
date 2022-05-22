using Newtonsoft.Json;

namespace isun.Entities.Http;

public class TokenRequestEntity
{
    public TokenRequestEntity(string username, string password)
    {
        Username = username;
        Password = password;
    }

    [JsonProperty("username")]
    public string Username { get; }


    [JsonProperty("password")]
    public string Password { get; }
}