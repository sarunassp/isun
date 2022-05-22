namespace isun.Entities.Http;

public class TokenEntity
{
    public TokenEntity(string token)
    {
        Token = token;
    }

    public string Token { get; }
}