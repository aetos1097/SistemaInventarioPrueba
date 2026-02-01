using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using ProductsSales.Application.DTOs;

namespace ProductsSales.WinForms.Services;

public class ApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ResiliencePipeline<HttpResponseMessage> _resiliencePipeline;
    private string? _token;
    private readonly string _baseUrl;

    public ApiClient(string baseUrl = "http://localhost:5134")
    {
        _baseUrl = baseUrl;
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(baseUrl),
            Timeout = TimeSpan.FromSeconds(30)
        };
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var retryOptions = new RetryStrategyOptions<HttpResponseMessage>
        {
            MaxRetryAttempts = 3,
            Delay = TimeSpan.FromSeconds(1),
            BackoffType = DelayBackoffType.Exponential,
            ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                .Handle<HttpRequestException>()
                .Handle<TaskCanceledException>()
                .HandleResult(r => (int)r.StatusCode >= 500 || r.StatusCode == HttpStatusCode.RequestTimeout)
        };

        var circuitBreakerOptions = new CircuitBreakerStrategyOptions<HttpResponseMessage>
        {
            FailureRatio = 0.5,
            MinimumThroughput = 5,
            BreakDuration = TimeSpan.FromSeconds(30),
            ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                .Handle<HttpRequestException>()
                .Handle<TaskCanceledException>()
                .HandleResult(r => !r.IsSuccessStatusCode)
        };

        _resiliencePipeline = new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddRetry(retryOptions)
            .AddCircuitBreaker(circuitBreakerOptions)
            .Build();
    }

    public void SetToken(string token)
    {
        _token = token;
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public void ClearToken()
    {
        _token = null;
        _httpClient.DefaultRequestHeaders.Authorization = null;
    }

    private async Task<HttpResponseMessage> ExecuteWithResilienceAsync(Func<CancellationToken, ValueTask<HttpResponseMessage>> action)
    {
        try
        {
            return await _resiliencePipeline.ExecuteAsync(action);
        }
        catch (BrokenCircuitException)
        {
            throw new Exception("El servicio no está disponible. Por favor, intente nuevamente en unos minutos.");
        }
    }

    public async Task<T?> GetAsync<T>(string endpoint)
    {
        try
        {
            var response = await ExecuteWithResilienceAsync(async ct =>
            {
                var result = await _httpClient.GetAsync(endpoint, ct);
                return result;
            });

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Error: {response.StatusCode} - {error}");
            }

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"Error al comunicarse con la API en {endpoint}: {ex.Message}", ex);
        }
        catch (TaskCanceledException)
        {
            throw new Exception($"Timeout al conectar con la API. Verifica que la API esté ejecutándose en {_baseUrl}");
        }
    }

    public async Task<T?> PostAsync<T>(string endpoint, object data)
    {
        try
        {
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            var json = JsonSerializer.Serialize(data, jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await ExecuteWithResilienceAsync(async ct =>
            {
                var result = await _httpClient.PostAsync(endpoint, content, ct);
                return result;
            });

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Error: {response.StatusCode} - {error}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(responseContent))
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"Error al comunicarse con la API en {endpoint}: {ex.Message}", ex);
        }
        catch (JsonException ex)
        {
            throw new Exception($"Error al procesar la respuesta del servidor: {ex.Message}", ex);
        }
        catch (TaskCanceledException)
        {
            throw new Exception($"Timeout al conectar con la API. Verifica que la API esté ejecutándose en {_baseUrl}");
        }
    }

    public async Task<T?> PutAsync<T>(string endpoint, object data)
    {
        try
        {
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            var json = JsonSerializer.Serialize(data, jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await ExecuteWithResilienceAsync(async ct =>
            {
                var result = await _httpClient.PutAsync(endpoint, content, ct);
                return result;
            });

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Error: {response.StatusCode} - {error}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"Error al comunicarse con la API en {endpoint}: {ex.Message}", ex);
        }
        catch (TaskCanceledException)
        {
            throw new Exception($"Timeout al conectar con la API. Verifica que la API esté ejecutándose en {_baseUrl}");
        }
    }

    /// <summary>Envía un archivo como multipart/form-data al endpoint. Devuelve la respuesta parseada.</summary>
    public async Task<T?> PostFileAsync<T>(string endpoint, string filePath, string formFieldName = "file")
    {
        try
        {
            var fileStream = File.OpenRead(filePath);
            var fileName = Path.GetFileName(filePath);
            var streamContent = new StreamContent(fileStream);
            var content = new MultipartFormDataContent();
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(GetContentType(fileName));
            content.Add(streamContent, formFieldName, fileName);

            var response = await ExecuteWithResilienceAsync(async ct =>
            {
                var result = await _httpClient.PostAsync(endpoint, content, ct);
                return result;
            });

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Error: {response.StatusCode} - {error}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(responseContent))
                return default;

            return JsonSerializer.Deserialize<T>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"Error al comunicarse con la API en {endpoint}: {ex.Message}", ex);
        }
        catch (TaskCanceledException)
        {
            throw new Exception($"Timeout al conectar con la API. Verifica que la API esté ejecutándose en {_baseUrl}");
        }
    }

    private static string GetContentType(string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return ext switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };
    }

    public async Task DeleteAsync(string endpoint)
    {
        try
        {
            var response = await ExecuteWithResilienceAsync(async ct =>
            {
                var result = await _httpClient.DeleteAsync(endpoint, ct);
                return result;
            });

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Error: {response.StatusCode} - {error}");
            }
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"Error al comunicarse con la API en {endpoint}: {ex.Message}", ex);
        }
        catch (TaskCanceledException)
        {
            throw new Exception($"Timeout al conectar con la API. Verifica que la API esté ejecutándose en {_baseUrl}");
        }
    }
}
