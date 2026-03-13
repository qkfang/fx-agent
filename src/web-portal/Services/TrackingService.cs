using FxWebPortal.Models;
using System.Text.Json;

namespace FxWebPortal.Services;

public class TrackingService
{
    private readonly string _filePath;
    private List<VisitorLog> _logs = new();
    private readonly object _lock = new();

    public TrackingService(IWebHostEnvironment env)
    {
        _filePath = Path.Combine(env.ContentRootPath, "Data", "visitors.json");
        Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
        Load();
    }

    private void Load()
    {
        lock (_lock)
        if (File.Exists(_filePath))
        {
            var json = File.ReadAllText(_filePath);
            _logs = JsonSerializer.Deserialize<List<VisitorLog>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
        }
    }

    private void Save()
    {
        var json = JsonSerializer.Serialize(_logs, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_filePath, json);
    }

    public Task AddLogAsync(TrackingRequest req)
    {
        lock (_lock)
        {
            var log = new VisitorLog
            {
                Id = _logs.Any() ? _logs.Max(l => l.Id) + 1 : 1,
                SessionId = req.SessionId,
                ArticleId = req.ArticleId,
                PageUrl = req.PageUrl,
                IpAddress = req.IpAddress,
                UserAgent = req.UserAgent,
                Referrer = req.Referrer,
                TimeSpentSeconds = req.TimeSpentSeconds,
                ClickCount = req.ClickCount,
                UserName = req.UserName,
                UserEmail = req.UserEmail,
                UserCompany = req.UserCompany,
                Language = req.Language,
                Timezone = req.Timezone,
                ScreenSize = req.ScreenSize,
                VisitedAt = DateTime.UtcNow
            };
            _logs.Add(log);
            Save();
        }
        return Task.CompletedTask;
    }

    public List<VisitorLog> GetAll()
    {
        lock (_lock) return _logs.OrderByDescending(l => l.VisitedAt).ToList();
    }

    public List<VisitorLog> GetLeads()
    {
        lock (_lock) return _logs
            .Where(l => !string.IsNullOrEmpty(l.UserEmail))
            .OrderByDescending(l => l.VisitedAt)
            .ToList();
    }

    public int GetUniqueSessionCount()
    {
        lock (_lock) return _logs.Select(l => l.SessionId).Distinct().Count();
    }

    public double GetAvgTimeSpent()
    {
        lock (_lock) return _logs.Any() ? _logs.Average(l => l.TimeSpentSeconds) : 0;
    }
}
