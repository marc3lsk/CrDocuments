using MessagePack;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Xml;
using WebApi.Features.Documents.Models;

namespace WebApi.Features.Documents.Controllers;

[ApiController]
[Route("[controller]")]
public class DocumentsController : ControllerBase
{
    static ConcurrentDictionary<string, string> _documents = new();

    [HttpGet]
    [Route("{id}")]
    public IActionResult Get(string id)
    {
        if (_documents.TryGetValue(id, out var jsonDocument))
        {
            if (Request.Headers.Accept.Contains("application/x-msgpack"))
            {
                return Ok(MessagePackSerializer.ConvertFromJson(jsonDocument));
            }
            if (Request.Headers.Accept.Contains("application/xml"))
            {
                Response.ContentType = "application/xml";
                XmlDocument? doc = JsonConvert.DeserializeXmlNode(jsonDocument, deserializeRootElementName: "document");
                return Ok(doc);
            }
            //var document = JsonSerializer.Deserialize<Document>(jsonDocument);
            return Ok(System.Text.Json.JsonSerializer.Deserialize<Document>(jsonDocument));
        }

        return NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Document document)
    {
        Request.Body.Seek(0, SeekOrigin.Begin);

        using var reader = new StreamReader(Request.Body);

        var x = await reader.ReadToEndAsync();
        if (_documents.TryAdd(document.id, x))
        {
            Response.StatusCode = StatusCodes.Status201Created;
            return Created();
        }

        throw new InvalidOperationException();
    }
}
