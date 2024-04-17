using MessagePack.AspNetCoreMvcFormatter;
using WebApi.Features.Documents.ResponseFormatters;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.AddControllers(options =>
{
    options.OutputFormatters.Add(new MessagePackOutputFormatter());
    options.OutputFormatters.Add(new DocumentXmlOutputFormatter());
})
    .AddXmlSerializerFormatters()
    .AddXmlDataContractSerializerFormatters();

var app = builder.Build();

app.MapControllers();

app.Run();
