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

        await docRef.SetAsync(JsonConvert.DeserializeObject<ExpandoObject>(documentEnvelope.RawJsonDocument));
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

        return JsonConvert.SerializeObject(snapshot.ToDictionary());
    }

    public async Task UpdateDocument(DocumentEnvelope documentEnvelope)
    {
        DocumentReference docRef = Documents.Document(documentEnvelope.DocumentMeta.id);

        await docRef.SetAsync(JsonConvert.DeserializeObject<ExpandoObject>(documentEnvelope.RawJsonDocument));
    }
}
