using System.Text.Json.Serialization;

namespace PersonalKnowledge.Domain.Dtos;

public class SpotifyPaginationResponse<T>
{
    [JsonPropertyName("href")]
    public string Href { get; set; }

    [JsonPropertyName("items")]
    public List<T> Items { get; set; }

    [JsonPropertyName("limit")]
    public int Limit { get; set; }

    [JsonPropertyName("next")]
    public string Next { get; set; }

    [JsonPropertyName("offset")]
    public int Offset { get; set; }

    [JsonPropertyName("previous")]
    public string Previous { get; set; }

    [JsonPropertyName("total")]
    public int Total { get; set; }
}

public class SpotifyItem
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("uri")]
    public string Uri { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }
}
