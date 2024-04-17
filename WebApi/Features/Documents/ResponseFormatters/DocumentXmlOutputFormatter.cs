using System.Text;
using System.Xml;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using WebApi.Features.Documents.Models;

namespace WebApi.Features.Documents.ResponseFormatters;

public class DocumentXmlOutputFormatter : TextOutputFormatter
{
    public DocumentXmlOutputFormatter()
    {
        SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/xml"));

        SupportedEncodings.Add(Encoding.UTF8);
        SupportedEncodings.Add(Encoding.Unicode);
    }
    protected override bool CanWriteType(Type? type)
        => typeof(Document).IsAssignableFrom(type)
            || typeof(IEnumerable<Document>).IsAssignableFrom(type);

    public override async Task WriteResponseBodyAsync(
        OutputFormatterWriteContext context, Encoding selectedEncoding)
    {
        var response = context.HttpContext.Response;

        XmlDocument? doc = JsonConvert.DeserializeXmlNode(JsonConvert.SerializeObject(context.Object!), deserializeRootElementName: "document");

        response.ContentType = "application/xml";
        await response.Body.WriteAsync(Encoding.UTF8.GetBytes(doc!.OuterXml));
    }
}
