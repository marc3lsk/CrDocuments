using MessagePack;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;
using System.Xml;
using WebApi.Features.Documents.Models;
using WebApi.Features.Documents.Persistence;

namespace WebApi.Features.Documents.Controllers;

[ApiController]
[Route("[controller]")]
public class DocumentsController : ControllerBase
{
    IDocumentRepository _documentRepository;

    public DocumentsController(
        IDocumentRepository documentRepository
    )
    {
        _documentRepository = documentRepository;
    }

    [HttpGet]
    [Route("{id}")]
    public async Task<IActionResult> Get(string id)
    {
        var documentEnvelope = await _documentRepository.GetDocument(id);

        if (documentEnvelope is null)
            return NotFound();

        if (Request.Headers.Accept.Contains("application/x-msgpack"))
        {
            return Ok(MessagePackSerializer.ConvertFromJson(documentEnvelope.JsonDocument));
        }
        if (Request.Headers.Accept.Contains("application/xml"))
        {
            XmlDocument? doc = JsonConvert.DeserializeXmlNode(documentEnvelope.JsonDocument, deserializeRootElementName: "document");
            return Ok(doc);
        }
        return Content(documentEnvelope.JsonDocument, "application/json; charset=utf-8");
    }

    async Task<(List<SchemaValidationEventArgs>, Document? document, string jsonDocument)> TryDeserializeDocumentFromRequestBody()
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

        return (errors, document, jsonDocument);
    }

    [HttpPost]
    public async Task<IActionResult> Post()
    {
        var (errors, document, jsonDocument) = await TryDeserializeDocumentFromRequestBody();

        if (errors.Any())
        {
            return BadRequest(new { Errors = errors.Select(err => new { err.Path, err.Message }) });
        }

        if (document is null)
        {
            return Problem();
        }

        if (await _documentRepository.DocumentAlreadyExists(document.id))
        {
            return Conflict("Document with same ID already exists");
        }

        await _documentRepository.CreateDocument(new DocumentEnvelope(document, jsonDocument));

        Response.StatusCode = StatusCodes.Status201Created;
        return Created();
    }

    [HttpPut]
    public async Task<IActionResult> Put()
    {
        var (errors, document, jsonDocument) = await TryDeserializeDocumentFromRequestBody();

        if (errors.Any())
        {
            return BadRequest(new { Errors = errors.Select(err => new { err.Path, err.Message }) });
        }

        if (document is null)
        {
            return Problem();
        }

        if (!await _documentRepository.DocumentAlreadyExists(document.id))
        {
            return NotFound();
        }

        await _documentRepository.UpdateDocument(new DocumentEnvelope(document, jsonDocument));

        return Ok();
    }
}
