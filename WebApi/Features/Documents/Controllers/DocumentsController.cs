﻿using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Schema;
using WebApi.Features.Documents.Models;
using WebApi.Features.Documents.Persistence;
using WebApi.Features.Documents.Helpers;

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

        return Ok(new RawDocumentJson(rawJsonDocument));
    }

    async Task<(List<SchemaValidationEventArgs>, DocumentMeta? documentMeta, string rawJsonDocument)> TryDeserializeAndValidateDocumentFromRequestBody()
    {
        using var bodyReader = new StreamReader(Request.Body);

        var rawJsonDocument = await bodyReader.ReadToEndAsync();

        var (errors, documentMeta) = RawDocumentJsonHelper.TryDeserializeAndValidateDocument(rawJsonDocument);

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
