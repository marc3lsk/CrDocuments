using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApi.Features.Documents.Models;
using WebApi.Features.Documents.Validation;

namespace UnitTests.Features.Documents.Validation;

public class DocumentSchemaValidationTests
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
        var (errors, _) = DocumentSchemaValidation.TryDeserializeAndValidateDocument(JsonConvert.SerializeObject(validDocument));

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
        var (errors, _) = DocumentSchemaValidation.TryDeserializeAndValidateDocument(JsonConvert.SerializeObject(invalidDocument));

        Assert.NotEmpty(errors);
    }
}
