using System.Net.Http.Json;
using WebApi.Features.Documents.Models;

namespace FunctionalTests.Features.Documents.Controllers;

public class DocumentsControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    CustomWebApplicationFactory<Program> _factory;

    public DocumentsControllerTests(
        CustomWebApplicationFactory<Program> factory
    )
    {
        _factory = factory;
    }

    [Fact]
    public async Task ValidJsonTest()
    {
        var client = _factory.CreateClient();

        var validDocument = new DocumentSchema(
            id: "1",
            tags: ["1"],
            data: "1"
        );

        var response = await client.PostAsJsonAsync("/documents", validDocument);

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task InvalidJsonTest()
    {
        var client = _factory.CreateClient();

        var invalidDocument = new { Something = "Something" };

        var response = await client.PostAsJsonAsync("/documents", invalidDocument);

        Assert.True(response.StatusCode == System.Net.HttpStatusCode.BadRequest);
    }
}
