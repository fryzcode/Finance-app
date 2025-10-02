using System.Net;
using FinanceApp.Api;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace FinanceApp.Api.Tests.Integration;

public class PublicEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public PublicEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(_ => { });
    }

    [Fact]
    public async Task AuthTest_Public_Returns_Ok()
    {
        using var client = _factory.CreateClient();
        var resp = await client.GetAsync("/api/AuthTest/public");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }
}


