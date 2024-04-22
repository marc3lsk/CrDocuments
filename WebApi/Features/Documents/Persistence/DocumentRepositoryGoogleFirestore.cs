using Google.Cloud.Firestore;
using Newtonsoft.Json;
using System.Dynamic;
using WebApi.Features.Documents.Models;

namespace WebApi.Features.Documents.Persistence;

public class DocumentRepositoryGoogleFirestore : IDocumentRepository
{
    CollectionReference Documents => FirestoreDb.Create("crdocuments-9daeb").Collection("documents");

    public async Task CreateDocument(DocumentEnvelope documentEnvelope)
    {
        DocumentReference docRef = Documents.Document(documentEnvelope.DocumentMeta.id);

        //await docRef.SetAsync(documentEnvelope);
        await docRef.SetAsync(new { documentEnvelope.DocumentMeta.id, documentEnvelope.DocumentMeta.tags, documentEnvelope.RawJsonDocument });
    }

    public async Task<bool> DocumentAlreadyExists(string documentId)
    {
        var docRef = Documents.Document(documentId);

        DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

        return snapshot.Exists;
    }

    public async Task<string?> GetRawJsonDocument(string documentId)
    {
        var docRef = Documents.Document(documentId);

        DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

        var values = snapshot.ToDictionary();

        return values.TryGetValue(nameof(DocumentEnvelope.RawJsonDocument), out var rawDocumentJson) ? rawDocumentJson as string : null;
    }

    public async Task UpdateDocument(DocumentEnvelope documentEnvelope)
    {
        DocumentReference docRef = Documents.Document(documentEnvelope.DocumentMeta.id);

        await docRef.SetAsync(new { documentEnvelope.DocumentMeta.id, documentEnvelope.DocumentMeta.tags, documentEnvelope.RawJsonDocument });
    }
}
