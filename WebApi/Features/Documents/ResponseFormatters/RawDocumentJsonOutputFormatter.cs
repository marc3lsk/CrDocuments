using System.Text;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using WebApi.Features.Documents.Helpers;
using WebApi.Features.Documents.Models;

namespace WebApi.Features.Documents.ResponseFormatters;

public class RawDocumentJsonOutputFormatter : TextOutputFormatter
{
    public RawDocumentJsonOutputFormatter()
    {
        SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/xml"));
        SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/json"));
        SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/x-msgpack"));

        SupportedEncodings.Add(Encoding.UTF8);
        SupportedEncodings.Add(Encoding.Unicode);
    }
    protected override bool CanWriteType(Type? type)
        => typeof(RawDocumentJson).IsAssignableFrom(type)
            || typeof(IEnumerable<RawDocumentJson>).IsAssignableFrom(type);

    public override async Task WriteResponseBodyAsync(
        OutputFormatterWriteContext context, Encoding selectedEncoding)
    {
        var response = context.HttpContext.Response;

        var documentRawJson = (RawDocumentJson)context.Object!;

        if (context.HttpContext.Request.Headers.Accept.Contains("application/x-msgpack"))
        {
            response.ContentType = "application/x-msgpack";
            await response.Body.WriteAsync(RawDocumentJsonHelper.ConvertToMsgPack(documentRawJson.RawJsonDocument));
        }
        if (context.HttpContext.Request.Headers.Accept.Contains("application/xml"))
        {
            response.ContentType = "application/xml; charset=utf-8";
            await response.Body.WriteAsync(Encoding.UTF8.GetBytes(RawDocumentJsonHelper.ConvertToXml(documentRawJson.RawJsonDocument)!.OuterXml));
        }
        if (context.HttpContext.Request.Headers.Accept.Contains("application/json"))
        {
            response.ContentType = "application/json; charset=utf-8";
            await response.Body.WriteAsync(Encoding.UTF8.GetBytes(documentRawJson.RawJsonDocument));
        }
    }
}
