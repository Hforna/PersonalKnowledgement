using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PersonalKnowledge.Infrastructure.Services;
using Xunit;

namespace PersonalKnowledge.Tests;

public class SpotifyServiceTests
{
    [Fact]
    public async Task GetIdsByNames_ShouldExtractCorrectIds()
    {
        // Arrange
        var jsonResponse = @"{
          ""href"": ""https://api.spotify.com/v1/me/shows?offset=0&limit=20"",
          ""limit"": 20,
          ""next"": ""https://api.spotify.com/v1/me/shows?offset=1&limit=1"",
          ""offset"": 0,
          ""previous"": ""https://api.spotify.com/v1/me/shows?offset=1&limit=1"",
          ""total"": 4,
          ""items"": [
            {
              ""id"": ""id_123"",
              ""name"": ""Show Name 1""
            },
            {
              ""id"": ""id_456"",
              ""name"": ""Show Name 2""
            }
          ]
        }";

        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(c => c.GetSection("services:tools:spotify:api_key").Value).Returns("fake_key");
        var mockHttpClientFactory = new Mock<IHttpClientFactory>();
        
        var service = new SpotifyService(mockConfig.Object, mockHttpClientFactory.Object, NullLogger<SpotifyService>.Instance);

        // Act
        var result = await service.GetIdsByNames(jsonResponse, new List<string> { "Show Name 1", "Show Name 2" });

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains("id_123", result);
        Assert.Contains("id_456", result);
    }
}
