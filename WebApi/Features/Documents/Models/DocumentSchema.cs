namespace WebApi.Features.Documents.Models;

public record DocumentSchema(
    string id,
    string[] tags,
    object data
);
