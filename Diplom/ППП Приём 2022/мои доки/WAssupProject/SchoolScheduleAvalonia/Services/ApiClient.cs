using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace SchoolScheduleAvalonia.Services;

public sealed class ApiClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    };

    public string BaseUrl { get; }

    public ApiClient(string baseUrl)
    {
        BaseUrl = baseUrl.TrimEnd('/');
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(BaseUrl + "/"),
            Timeout = TimeSpan.FromSeconds(20)
        };
    }

    public async Task<List<T>> GetListAsync<T>(string endpoint, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync(endpoint, cancellationToken);
        await EnsureSuccessAsync(response, endpoint);

        var data = await response.Content.ReadFromJsonAsync<List<T>>(_jsonOptions, cancellationToken);
        return data ?? new List<T>();
    }

    public async Task<T?> GetAsync<T>(string endpoint, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync(endpoint, cancellationToken);
        await EnsureSuccessAsync(response, endpoint);

        return await response.Content.ReadFromJsonAsync<T>(_jsonOptions, cancellationToken);
    }

    public async Task PostAsync<T>(string endpoint, T item, CancellationToken cancellationToken = default)
    {
        using var content = CreateJsonContent(item);
        using var response = await _httpClient.PostAsync(endpoint, content, cancellationToken);
        await EnsureSuccessAsync(response, endpoint);
    }

    public async Task PutAsync<T>(string endpoint, T item, CancellationToken cancellationToken = default)
    {
        using var content = CreateJsonContent(item);
        using var response = await _httpClient.PutAsync(endpoint, content, cancellationToken);
        await EnsureSuccessAsync(response, endpoint);
    }

    public async Task DeleteAsync(string endpoint, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.DeleteAsync(endpoint, cancellationToken);
        await EnsureSuccessAsync(response, endpoint);
    }

    private HttpContent CreateJsonContent<T>(T item)
    {
        var json = JsonSerializer.Serialize(item, _jsonOptions);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, string endpoint)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var body = await response.Content.ReadAsStringAsync();
        var message = string.IsNullOrWhiteSpace(body)
            ? $"API вернул ошибку {(int)response.StatusCode} {response.ReasonPhrase} для endpoint '{endpoint}'."
            : $"API вернул ошибку {(int)response.StatusCode} {response.ReasonPhrase} для endpoint '{endpoint}'. Ответ: {body}";

        throw new InvalidOperationException(message);
    }
}
