using isun.Configuration;
using isun.Entities.Http;
using isun.Exceptions;
using Newtonsoft.Json;

namespace isun.Http.Clients;

public interface IWeatherClient
{
    Task<IEnumerable<string>> GetCitiesAsync();
    Task<WeatherEntity> GetWeatherAsync(string city);
}

public class WeatherClient : IWeatherClient
{
    private readonly HttpClient _httpClient;

    public WeatherClient(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient(Constants.HttpClientNames.AuthenticatedWeatherClient);
    }

    public async Task<IEnumerable<string>> GetCitiesAsync()
    {
        var response = await _httpClient.GetAsync("cities");

        return await DeserializeResponse<IEnumerable<string>>(response);
    }

    public async Task<WeatherEntity> GetWeatherAsync(string city)
    {
        var response = await _httpClient.GetAsync($"weathers/{Uri.EscapeDataString(city)}");

        return await DeserializeResponse<WeatherEntity>(response);
    }

    private async Task<T> DeserializeResponse<T>(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode == false)
        {
            throw new StatusCodeException(response.StatusCode);
        }

        var responseContent = JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync());
        return responseContent;
    }
}