﻿using MessagePack;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;
using System.Xml;
using WebApi.Features.Documents.Models;

namespace WebApi.Features.Documents.Helpers;

public class RawDocumentJsonHelper
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

    public static byte[] ConvertToMsgPack(string rawJsonDocument)
    {
        return MessagePackSerializer.ConvertFromJson(rawJsonDocument);
    }

    public static XmlDocument? ConvertToXml(string rawJsonDocument)
    {
        return JsonConvert.DeserializeXmlNode(rawJsonDocument, deserializeRootElementName: "document");
    }
}
