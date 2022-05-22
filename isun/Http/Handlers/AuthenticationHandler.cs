using System.Text;
using isun.Configuration;
using isun.Entities.Http;
using isun.Exceptions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace isun.Http.Handlers;

public class AuthenticationHandler : DelegatingHandler
{
    private readonly HttpClient _httpClient;
    private readonly WeatherClientSettings _settings;
    private readonly ILogger<AuthenticationHandler> _logger;
    private string _authToken;

    public AuthenticationHandler(IHttpClientFactory httpClientFactory, WeatherClientSettings settings, ILogger<AuthenticationHandler> logger)
    {
        _settings = settings;
        _logger = logger;

        _httpClient = httpClientFactory.CreateClient(Constants.HttpClientNames.BasicWeatherClient);
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Headers.Add("Authorization", await GetToken());
        return await base.SendAsync(request, cancellationToken);
    }

    private async Task<string> GetToken()
    {
        if (_authToken != null)
            return _authToken;

        var tokenRequest = new TokenRequestEntity(_settings.Username, _settings.Password);

        var response = await _httpClient.PostAsync(
            "authorize",
            new StringContent(JsonConvert.SerializeObject(tokenRequest), Encoding.Default, "application/json"));

        if (response.IsSuccessStatusCode == false)
        {
            _logger.LogError("Failed to authenticate to weather API, response: {@response}", response);
            throw new StatusCodeException(response.StatusCode);
        }

        var token = JsonConvert.DeserializeObject<TokenEntity>(await response.Content.ReadAsStringAsync());
        _authToken = token.Token;

        return _authToken;
    }
}