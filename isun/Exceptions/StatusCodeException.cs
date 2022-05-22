using System.Net;

namespace isun.Exceptions;

public class StatusCodeException : Exception
{
    public readonly HttpStatusCode _statusCode;

    public StatusCodeException(HttpStatusCode statusCode)
    {
        _statusCode = statusCode;
    }
}