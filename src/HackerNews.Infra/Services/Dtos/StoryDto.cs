using System.Text.Json.Serialization;

namespace HackerNews.Infra.Services.Dtos;

public class StoryDto
{
    [JsonPropertyName("by")] public string? By { get; set; }
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("descendants")] public int? Descendants { get; set; }
    [JsonPropertyName("score")] public int? Score { get; set; }
    [JsonPropertyName("time")] public long? Time { get; set; }
    [JsonPropertyName("title")] public string? Title { get; set; }
    [JsonPropertyName("url")] public string? Url { get; set; }
}
