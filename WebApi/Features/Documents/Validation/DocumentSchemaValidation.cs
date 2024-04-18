using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;
using WebApi.Features.Documents.Models;

namespace WebApi.Features.Documents.Validation;

public class DocumentSchemaValidation
{
    public static (List<SchemaValidationEventArgs>, DocumentMeta? documentMeta) TryDeserializeAndValidateDocument(string rawJsonDocument)
    {
        JSchemaGenerator generator = new JSchemaGenerator();

        JSchema schema = generator.Generate(typeof(DocumentSchema));

        schema.AllowAdditionalProperties = false;

        JsonTextReader jsonReader = new JsonTextReader(new StringReader(rawJsonDocument));

        JSchemaValidatingReader validatingReader = new JSchemaValidatingReader(jsonReader);
        validatingReader.Schema = schema;

        var errors = new List<SchemaValidationEventArgs>();
        validatingReader.ValidationEventHandler += (o, a) => errors.Add(a);
        JsonSerializer serializer = new JsonSerializer();
        var documentMeta = serializer.Deserialize<DocumentMeta>(validatingReader);

        return (errors, documentMeta);
    }
}
