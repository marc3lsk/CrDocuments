using Serilog;
using WebApi.Features.Documents.Persistence;
using WebApi.Features.Documents.ResponseFormatters;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Host.UseSerilog((context, config) =>
{
    config.ReadFrom.Configuration(context.Configuration);
});

builder.Services.AddScoped<IDocumentRepository, DocumentRepositoryFileSystem>();

builder.Services.AddControllers(options =>
{
    options.OutputFormatters.Insert(0, new RawDocumentJsonOutputFormatter()); // position 0 to override default json formatter
});

var app = builder.Build();

app.UseSerilogRequestLogging();

app.MapControllers();

app.Run();

public partial class Program { }
