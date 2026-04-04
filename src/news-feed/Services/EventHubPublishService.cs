using Azure.Identity;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using FxWebNews.Models;
using System.Text;
using System.Text.Json;

namespace FxWebNews.Services
{
    public class EventHubPublishService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EventHubPublishService> _logger;

        public EventHubPublishService(IConfiguration config, ILogger<EventHubPublishService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task<(bool Success, string Message, int SentCount)> PublishBatchAsync(List<NewsArticle> articles)
        {
            var fullyQualifiedNamespace = _config["EventHub:FullyQualifiedNamespace"];
            var eventHubName = _config["EventHub:EventHubName"];

            if (string.IsNullOrWhiteSpace(fullyQualifiedNamespace))
            {
                return (false, "EventHub:FullyQualifiedNamespace is not configured.", 0);
            }

            try
            {
                await using var producerClient = new EventHubProducerClient(fullyQualifiedNamespace, eventHubName, new DefaultAzureCredential());

                using EventDataBatch eventBatch = await producerClient.CreateBatchAsync();

                int addedCount = 0;
                foreach (var article in articles)
                {
                    var payload = new
                    {
                        id = article.Id,
                        title = article.Title,
                        summary = article.Summary,
                        content = article.Content,
                        type = article.Type,
                        category = article.Category,
                        author = article.Author,
                        publishedDate = article.PublishedDate,
                        publishedAt = article.PublishedAt,
                        source = article.Source
                    };

                    var json = JsonSerializer.Serialize(payload);
                    var eventData = new EventData(Encoding.UTF8.GetBytes(json));

                    if (!eventBatch.TryAdd(eventData))
                    {
                        _logger.LogWarning("Article {Id} could not be added to the batch (batch size limit reached).", article.Id);
                        break;
                    }

                    addedCount++;
                }

                if (addedCount == 0)
                {
                    return (false, "No articles could be added to the event batch.", 0);
                }

                await producerClient.SendAsync(eventBatch);

                _logger.LogInformation("Successfully sent {Count} articles to Event Hub.", addedCount);
                return (true, $"Successfully sent {addedCount} article(s) to Fabric Event Hub.", addedCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish articles to Event Hub.");
                return (false, $"Failed to publish to Event Hub: {ex.Message}", 0);
            }
        }
    }
}
