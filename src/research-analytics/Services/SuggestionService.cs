using FxWebPortal.Models;
using System.Text.Json;

namespace FxWebPortal.Services;

public class SuggestionService
{
    private readonly string _filePath;
    private List<CustomerSuggestion> _suggestions = new();
    private int _nextId;

    public SuggestionService(IWebHostEnvironment env)
    {
        _filePath = Path.Combine(env.ContentRootPath, "Data", "suggestions.json");
        Load();
    }

    private void Load()
    {
        if (File.Exists(_filePath))
        {
            var json = File.ReadAllText(_filePath);
            _suggestions = JsonSerializer.Deserialize<List<CustomerSuggestion>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();

            foreach (var s in _suggestions)
            {
                if (s.ReceivedAt == default)
                    s.ReceivedAt = DateTime.UtcNow;
            }
        }
        _nextId = _suggestions.Count > 0 ? _suggestions.Max(s => s.Id) + 1 : 1;
    }

    public List<CustomerSuggestion> GetAll() => _suggestions.OrderByDescending(s => s.ReceivedAt).ToList();

    public CustomerSuggestion? GetById(int id) => _suggestions.FirstOrDefault(s => s.Id == id);

    public CustomerSuggestion Add(CustomerSuggestion suggestion)
    {
        suggestion.Id = _nextId++;
        suggestion.ReceivedAt = DateTime.UtcNow;
        _suggestions.Add(suggestion);
        return suggestion;
    }
}
