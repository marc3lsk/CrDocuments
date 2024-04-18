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
        if (!await _documentRepository.DocumentAlreadyExists(id))
        {
            return NotFound();
        }

        var rawJsonDocument = await _documentRepository.GetRawJsonDocument(id);

        if (rawJsonDocument is null)
            return Problem("Failed to retrieve Document");

        if (Request.Headers.Accept.Contains("application/x-msgpack"))
        {
            return Ok(MessagePackSerializer.ConvertFromJson(rawJsonDocument));
        }
        if (Request.Headers.Accept.Contains("application/xml"))
        {
            XmlDocument? doc = JsonConvert.DeserializeXmlNode(rawJsonDocument, deserializeRootElementName: "document");
            return Ok(doc);
        }
        return Content(rawJsonDocument, "application/json; charset=utf-8");
    }

    async Task<(List<SchemaValidationEventArgs>, DocumentMeta? documentMeta, string rawJsonDocument)> TryDeserializeAndValidateDocumentFromRequestBody()
    {
        JSchemaGenerator generator = new JSchemaGenerator();

        JSchema schema = generator.Generate(typeof(DocumentSchema));

        schema.AllowAdditionalProperties = false;

        using var bodyReader = new StreamReader(Request.Body);

        var rawJsonDocument = await bodyReader.ReadToEndAsync();

        JsonTextReader jsonReader = new JsonTextReader(new StringReader(rawJsonDocument));

        JSchemaValidatingReader validatingReader = new JSchemaValidatingReader(jsonReader);
        validatingReader.Schema = schema;

        var errors = new List<SchemaValidationEventArgs>();
        validatingReader.ValidationEventHandler += (o, a) => errors.Add(a);
        JsonSerializer serializer = new JsonSerializer();
        var documentMeta = serializer.Deserialize<DocumentMeta>(validatingReader);

        return (errors, documentMeta, rawJsonDocument);
    }

    [HttpPost]
    public async Task<IActionResult> Post()
    {
        var (errors, documentMeta, rawJsonDocument) = await TryDeserializeAndValidateDocumentFromRequestBody();

        if (errors.Any())
        {
            return BadRequest(new { Errors = errors.Select(err => new { err.Path, err.Message }) });
        }

        if (documentMeta is null)
        {
            return Problem();
        }

        if (await _documentRepository.DocumentAlreadyExists(documentMeta.id))
        {
            return Conflict("Document with same ID already exists");
        }

        await _documentRepository.CreateDocument(new DocumentEnvelope(documentMeta, rawJsonDocument));

        Response.StatusCode = StatusCodes.Status201Created;
        return Created();
    }

    [HttpPut]
    public async Task<IActionResult> Put()
    {
        var (errors, documentMeta, rawJsonDocument) = await TryDeserializeAndValidateDocumentFromRequestBody();

        if (errors.Any())
        {
            return BadRequest(new { Errors = errors.Select(err => new { err.Path, err.Message }) });
        }

        if (documentMeta is null)
        {
            return Problem();
        }

        if (!await _documentRepository.DocumentAlreadyExists(documentMeta.id))
        {
            return NotFound();
        }

        await _documentRepository.UpdateDocument(new DocumentEnvelope(documentMeta, rawJsonDocument));

        return Ok();
    }
}
