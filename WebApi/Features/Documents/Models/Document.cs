using MessagePack;

namespace WebApi.Features.Documents.Models;

[MessagePackObject(keyAsPropertyName: true)]
public record Document(string id, string[] tags, object data);
