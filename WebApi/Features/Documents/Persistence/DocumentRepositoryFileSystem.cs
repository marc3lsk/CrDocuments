using System.Security.Cryptography;
using System.Text;
using WebApi.Features.Documents.Models;

namespace WebApi.Features.Documents.Persistence;

public class DocumentRepositoryFileSystem : IDocumentRepository
{
    const string STORAGE_PATH = @"c:/temp/documents";

    static string DocumentPath(string documentId) => Path.Combine(STORAGE_PATH, ToValidUniqueFileName(documentId));

    static string ToValidUniqueFileName(string input)
    {
        // Compute hash
        byte[] bytes = Encoding.UTF8.GetBytes(input);
        byte[] hashBytes;
        using (SHA256 sha256 = SHA256.Create())
        {
            hashBytes = sha256.ComputeHash(bytes);
        }

        // Convert hash to a valid file name (hexadecimal representation)
        StringBuilder sb = new StringBuilder();
        sb.AppendJoin("", hashBytes.Select(x => x.ToString("x2") /* Convert byte to two-digit hexadecimal representation */));
        return sb.ToString();
    }

    public async Task CreateDocument(DocumentEnvelope documentEnvelope)
    {
        await File.WriteAllTextAsync(DocumentPath(documentEnvelope.DocumentMeta.id), documentEnvelope.RawJsonDocument);
    }

    public Task<bool> DocumentAlreadyExists(string documentId)
    {
        return Task.FromResult(File.Exists(DocumentPath(documentId)));
    }

    public Task<string?> GetRawJsonDocument(string documentId)
    {
        return File.ReadAllTextAsync(DocumentPath(documentId))!;
    }

    public async Task UpdateDocument(DocumentEnvelope documentEnvelope)
    {
        await File.WriteAllTextAsync(DocumentPath(documentEnvelope.DocumentMeta.id), documentEnvelope.RawJsonDocument);
    }
}
