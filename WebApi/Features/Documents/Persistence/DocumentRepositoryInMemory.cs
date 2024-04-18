using System.Collections.Concurrent;
using WebApi.Features.Documents.Models;

namespace WebApi.Features.Documents.Persistence;

public class DocumentRepositoryInMemory : IDocumentRepository
{
    static ConcurrentDictionary<string, DocumentEnvelope> _documents = new();

    public Task CreateDocument(DocumentEnvelope documentEnvelope)
    {
        _documents[documentEnvelope.DocumentMeta.id] = documentEnvelope;
        return Task.CompletedTask;
    }

    public Task<bool> DocumentAlreadyExists(string documentId)
    {
        return Task.FromResult(_documents.ContainsKey(documentId));
    }

    public Task<DocumentEnvelope?> GetDocument(string documentId)
    {
        return Task.FromResult(_documents.TryGetValue(documentId, out var documentEnvelope) ? documentEnvelope : null);
    }

    public Task UpdateDocument(DocumentEnvelope documentEnvelope)
    {
        _documents[documentEnvelope.DocumentMeta.id] = documentEnvelope;
        return Task.CompletedTask;
    }
}
