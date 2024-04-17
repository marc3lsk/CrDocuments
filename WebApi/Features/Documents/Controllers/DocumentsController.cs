using MessagePack;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;
using System.Collections.Concurrent;
using System.Xml;
using WebApi.Features.Documents.Models;

namespace WebApi.Features.Documents.Controllers;

[ApiController]
[Route("[controller]")]
public class DocumentsController : ControllerBase
{
    static ConcurrentDictionary<string, DocumentEnvelope> _documents = new();

    [HttpGet]
    [Route("{id}")]
    public IActionResult Get(string id)
    {
        if (_documents.TryGetValue(id, out var documentEnvelope))
        {
            if (Request.Headers.Accept.Contains("application/x-msgpack"))
            {
                return Ok(MessagePackSerializer.ConvertFromJson(documentEnvelope.JsonDocument));
            }
            if (Request.Headers.Accept.Contains("application/xml"))
            {
                XmlDocument? doc = JsonConvert.DeserializeXmlNode(documentEnvelope.JsonDocument, deserializeRootElementName: "document");
                return Ok(doc);
            }
            return Ok(documentEnvelope.Document);
        }

        return NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> Post()
    {
        JSchemaGenerator generator = new JSchemaGenerator();

        JSchema schema = generator.Generate(typeof(Document));

        schema.AllowAdditionalProperties = false;

        using var bodyReader = new StreamReader(Request.Body);

        var jsonDocument = await bodyReader.ReadToEndAsync();

        JsonTextReader jsonReader = new JsonTextReader(new StringReader(jsonDocument));

        JSchemaValidatingReader validatingReader = new JSchemaValidatingReader(jsonReader);
        validatingReader.Schema = schema;

        var errors = new List<SchemaValidationEventArgs>();
        validatingReader.ValidationEventHandler += (o, a) => errors.Add(a);
        JsonSerializer serializer = new JsonSerializer();
        var document = serializer.Deserialize<Document>(validatingReader);

        if (errors.Any())
        {
            return BadRequest(errors.Select(err => new { err.Path, err.Message }));
        }

        if (_documents.TryAdd(document.id, new DocumentEnvelope(document, jsonDocument)))
        {
            Response.StatusCode = StatusCodes.Status201Created;
            return Created();
        }

        throw new InvalidOperationException();
    }
}
