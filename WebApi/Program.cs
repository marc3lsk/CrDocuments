using MessagePack.AspNetCoreMvcFormatter;
using Serilog;
using WebApi.Features.Documents.Persistence;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Host.UseSerilog((context, config) =>
{
    config.ReadFrom.Configuration(context.Configuration);
});

builder.Services.AddScoped<IDocumentRepository, DocumentRepositoryInMemory>();

builder.Services.AddControllers(options =>
{
    options.OutputFormatters.Add(new MessagePackOutputFormatter());
    //options.OutputFormatters.Add(new DocumentXmlOutputFormatter());
})
    .AddXmlSerializerFormatters()
    .AddXmlDataContractSerializerFormatters();

var app = builder.Build();

app.UseSerilogRequestLogging();

app.MapControllers();

app.Run();
