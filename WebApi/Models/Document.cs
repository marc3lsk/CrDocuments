using MessagePack;

namespace WebApi.Models;

[MessagePackObject(keyAsPropertyName: true)]
public record Document(string id, string[] tags, dynamic? data);
