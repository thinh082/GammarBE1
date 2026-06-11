namespace GammarDomain.Entities;

public class ApiSetting
{
    public long Id { get; private set; }
    public string ApiKey { get; private set; } = string.Empty;
    public string Provider { get; private set; } = string.Empty;
    public string? BaseUrl { get; private set; }
    public string? ModelName { get; private set; }
    public decimal Temperature { get; private set; }
    public int? MaxTokens { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private ApiSetting()
    {
    }

    public ApiSetting(string apiKey, string provider, string? baseUrl = null, string? modelName = null, decimal temperature = 1m, int? maxTokens = null)
    {
        ApiKey = apiKey;
        Provider = provider;
        BaseUrl = baseUrl;
        ModelName = modelName;
        Temperature = temperature;
        MaxTokens = maxTokens;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
