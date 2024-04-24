using MessagePack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

        var id = Guid.NewGuid().ToString();

        var validDocument = new DocumentSchema(
            id: id,
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

    [Fact]
    public async Task GetMsgPack()
    {
        var client = _factory.CreateClient();

        var id = Guid.NewGuid().ToString();

        var validDocument = new DocumentSchema(
            id: id,
            tags: ["1", "2", "3"],
            data: "1"
        );

        var response = await client.PostAsJsonAsync("/documents", validDocument);

        response.EnsureSuccessStatusCode();

        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/x-msgpack"));

        var msgPackBytesResponse = await client.GetAsync($"/documents/{id}");

        var msgPackBytes = await msgPackBytesResponse.Content.ReadAsByteArrayAsync();

        var document = MessagePackSerializer.Deserialize<DocumentSchema>(msgPackBytes);

        Assert.Equal(validDocument.id, document.id);
        Assert.Equal(validDocument.tags, document.tags);
        Assert.Equal(validDocument.data, document.data);
    }

    [Fact]
    public async Task GetXml()
    {
        var client = _factory.CreateClient();

        var id = Guid.NewGuid().ToString();

        var validDocument = new DocumentSchema
        (
            id: id,
            tags: ["1", "2", "3"],
            data: new { something = "cool" }
        );

        var response = await client.PostAsJsonAsync("/documents", validDocument);

        response.EnsureSuccessStatusCode();

        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/xml"));

        var xmlResponse = await client.GetAsync($"/documents/{id}");

        var rawXml = await xmlResponse.Content.ReadAsStringAsync();

        Assert.Equal($"<document><id>{id}</id><tags>1</tags><tags>2</tags><tags>3</tags><data><something>cool</something></data></document>", rawXml);
    }

    [Fact]
    public async Task GetJson()
    {
        var client = _factory.CreateClient();

        var id = Guid.NewGuid().ToString();

        var validDocument = new DocumentSchema
        (
            id: id,
            tags: ["1", "2", "3"],
            data: new { something = "cool" }
        );

        var response = await client.PostAsJsonAsync("/documents", validDocument);

        response.EnsureSuccessStatusCode();

        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

        var jsonResponse = await client.GetAsync($"/documents/{id}");

        var rawJson = await jsonResponse.Content.ReadAsStringAsync();

        JToken referenceToken = JToken.Parse(JsonConvert.SerializeObject(validDocument));
        JToken tokenFromResponse = JToken.Parse(rawJson);

        Assert.True(JToken.DeepEquals(referenceToken, tokenFromResponse));
    }
}
