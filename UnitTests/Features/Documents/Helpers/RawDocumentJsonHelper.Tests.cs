using Newtonsoft.Json;
using WebApi.Features.Documents.Models;
using WebApi.Features.Documents.Helpers;

namespace UnitTests.Features.Documents.Helpers;

public class RawDocumentJsonHelperTests
{
    [Fact]
    public void ValidDocument()
    {
        var validDocument = new DocumentSchema
        (
            id: "123",
            tags: ["1", "2", "3"],
            data: new { something = "cool" }
        );
        var (errors, _) = RawDocumentJsonHelper.TryDeserializeAndValidateDocument(JsonConvert.SerializeObject(validDocument));

        Assert.Empty(errors);
    }

    [Fact]
    public void InvalidDocument()
    {
        var invalidDocument = new
        {
            tags = (string[])["1", "2", "3"],
            data = new { something = "cool" }
        };
        var (errors, _) = RawDocumentJsonHelper.TryDeserializeAndValidateDocument(JsonConvert.SerializeObject(invalidDocument));

        Assert.NotEmpty(errors);
    }

    [Fact]
    public void Xml()
    {
        var validDocument = new DocumentSchema
        (
            id: "123",
            tags: ["1", "2", "3"],
            data: new { something = "cool" }
        );
        var doc = RawDocumentJsonHelper.ConvertToXml(JsonConvert.SerializeObject(validDocument));

        Assert.Equal("<document><id>123</id><tags>1</tags><tags>2</tags><tags>3</tags><data><something>cool</something></data></document>", doc!.OuterXml);
    }
}
