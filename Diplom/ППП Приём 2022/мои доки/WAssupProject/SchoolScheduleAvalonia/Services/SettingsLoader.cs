using System.Text.Json;

namespace SchoolScheduleAvalonia.Services;

public static class SettingsLoader
{
    private const string DefaultApiBaseUrl = "http://localhost:54040/api";

    public static string LoadApiBaseUrl()
    {
        try
        {
            var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
            if (!File.Exists(path))
            {
                return DefaultApiBaseUrl;
            }

            using var document = JsonDocument.Parse(File.ReadAllText(path));
            if (document.RootElement.TryGetProperty("ApiSettings", out var apiSettings) &&
                apiSettings.TryGetProperty("BaseUrl", out var baseUrlElement))
            {
                var value = baseUrlElement.GetString();
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value.TrimEnd('/');
                }
            }
        }
        catch
        {
            // Если настройки повреждены, приложение остается работоспособным на стандартном адресе API.
        }

        return DefaultApiBaseUrl;
    }
}
