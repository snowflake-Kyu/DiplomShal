using System.Net.Http.Json;

namespace SchoolScheduleWeb.Services;

public class ApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ApiClient> _logger;

    public string BaseUrl => _httpClient.BaseAddress?.ToString() ?? string.Empty;

    public ApiClient(HttpClient httpClient, ILogger<ApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<T>> GetListAsync<T>(string endpoint, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<List<T>>(endpoint, cancellationToken);
            return result ?? new List<T>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Ошибка HTTP при запросе к API endpoint={Endpoint}", endpoint);
            throw new InvalidOperationException($"Не удалось получить данные из API: {endpoint}. Проверьте, что API запущен и доступен.", ex);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Превышено время ожидания ответа API endpoint={Endpoint}", endpoint);
            throw new InvalidOperationException($"API не ответил вовремя: {endpoint}.", ex);
        }
    }

    public async Task<T?> GetItemAsync<T>(string endpoint, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<T>(endpoint, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Ошибка HTTP при запросе к API endpoint={Endpoint}", endpoint);
            throw new InvalidOperationException($"Не удалось получить запись из API: {endpoint}. Проверьте, что API запущен и доступен.", ex);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Превышено время ожидания ответа API endpoint={Endpoint}", endpoint);
            throw new InvalidOperationException($"API не ответил вовремя: {endpoint}.", ex);
        }
    }
}
