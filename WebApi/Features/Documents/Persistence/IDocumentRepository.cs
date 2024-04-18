using WebApi.Features.Documents.Models;

namespace WebApi.Features.Documents.Persistence;

public interface IDocumentRepository
{
    Task CreateDocument(DocumentEnvelope documentEnvelope);

    Task<bool> DocumentAlreadyExists(string documentId);

    Task UpdateDocument(DocumentEnvelope documentEnvelope);

    Task<string?> GetRawJsonDocument(string documentId);
}
