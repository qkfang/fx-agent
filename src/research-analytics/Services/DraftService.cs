using FxWebPortal.Models;
using System.Text.Json;

namespace FxWebPortal.Services;

public class DraftService
{
    private readonly HttpClient _http;
    private readonly string _baseUrl;
    private readonly ILogger<DraftService> _logger;
    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public DraftService(HttpClient http, IConfiguration config, ILogger<DraftService> logger)
    {
        _http = http;
        _baseUrl = config["IntegrationApi:BaseUrl"]?.TrimEnd('/') ?? "http://localhost:5005";
        _logger = logger;
    }

    public List<ResearchDraft> GetAll()
    {
        try
        {
            var response = _http.GetAsync($"{_baseUrl}/api/researchdrafts").Result;
            response.EnsureSuccessStatusCode();
            var json = response.Content.ReadAsStringAsync().Result;
            return JsonSerializer.Deserialize<List<ResearchDraft>>(json, _jsonOptions)?
                .OrderByDescending(d => d.CreatedAt).ToList() ?? new();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch drafts from integration API");
            return new();
        }
    }

    public ResearchDraft? GetById(int id)
    {
        try
        {
            var response = _http.GetAsync($"{_baseUrl}/api/researchdrafts/{id}").Result;
            if (!response.IsSuccessStatusCode) return null;
            var json = response.Content.ReadAsStringAsync().Result;
            return JsonSerializer.Deserialize<ResearchDraft>(json, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch draft {Id}", id);
            return null;
        }
    }

    public bool Delete(int id)
    {
        try
        {
            var response = _http.DeleteAsync($"{_baseUrl}/api/researchdrafts/{id}").Result;
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete draft {Id}", id);
            return false;
        }
    }
}
