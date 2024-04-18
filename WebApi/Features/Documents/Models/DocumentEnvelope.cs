namespace WebApi.Features.Documents.Models;

public record DocumentEnvelope(
    DocumentMeta DocumentMeta,
    string RawJsonDocument
);
