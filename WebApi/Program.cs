using MessagePack.AspNetCoreMvcFormatter;
using Serilog;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Host.UseSerilog((context, config) =>
{
    config.ReadFrom.Configuration(context.Configuration);
});

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
