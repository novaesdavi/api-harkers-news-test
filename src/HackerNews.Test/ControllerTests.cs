using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using HackerNews.Infra.Services;
using HackerNews.Api;
using HackerNews.Domain.Entities;

namespace HackerNews.Test;

[TestClass]
public sealed class ControllerTests
{
    // USando MSTest
    [TestMethod]
    public async Task GetBestTopStories_ReturnsOnlyOneItem()
    {
        // Arrange
       using var factory = ControllerFactoryWebApplication();

        var client = factory.CreateClient();
        // Act
        var resp = await client.GetAsync("/v0/story/beststories?n=3");
        // Assert
        resp.EnsureSuccessStatusCode();
        var content = await resp.Content.ReadAsStringAsync();

        var stories = System.Text.Json.JsonSerializer.Deserialize<List<Story>>(content!);
        Assert.IsNotNull(content);
        Assert.AreEqual(3,stories.Count);
        Assert.AreEqual("title", stories[0].Title);
        Assert.AreEqual("author", stories[0].PostedBy);
        Assert.AreEqual("http://example\"", stories[0].Uri);



    }
    public WebApplicationFactory<Program> ControllerFactoryWebApplication()
    {
        var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    var mockApi = new Mock<IHackerNewsApi>();
                    mockApi.Setup(x => x.GetBestStoriesIds()).ReturnsAsync(new[] { 1, 2, 3 });
                    mockApi.Setup(x => x.GetItem(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(new HackerNews.Infra.Services.Dtos.StoryDto { Id = 1, Title = "title", By = "author", Url = "http://example", Score = 1, Descendants = 0, Time = 0 });
                    services.AddSingleton(mockApi.Object);
                });
            });

        return factory;
    }
}
