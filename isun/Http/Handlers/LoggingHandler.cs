using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace isun.Http.Handlers;

public class LoggingHandler : DelegatingHandler
{
    private readonly ILogger<LoggingHandler> _logger;

    public LoggingHandler(ILogger<LoggingHandler> logger)
    {
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Request: {Uri} {Method}, body: {Content}",
                request.RequestUri,
                request.Method,
                await ReadContent(request.Content, cancellationToken));

            var response = await base.SendAsync(request, cancellationToken);
            stopwatch.Stop();
            _logger.LogInformation("Response: {Uri} {Method} returned {StatusCode} in {Elapsed}ms, body: {Content}",
                request.RequestUri,
                request.Method,
                response.StatusCode,
                stopwatch.ElapsedMilliseconds,
                await ReadContent(response.Content, cancellationToken));

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Request: {Uri} {Method} failed unexpectedly.", request.RequestUri, request.Method);
            throw;
        }
    }

    private static async Task<string> ReadContent(HttpContent content, CancellationToken cancellationToken)
    {
        if (content == null)
            return null;

        return await content.ReadAsStringAsync(cancellationToken);
    }
}