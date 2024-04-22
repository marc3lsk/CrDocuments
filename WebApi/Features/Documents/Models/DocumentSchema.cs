using MessagePack;

namespace WebApi.Features.Documents.Models;

[MessagePackObject(keyAsPropertyName: true)]
public record DocumentSchema(
    string id,
    string[] tags,
    object data
);
