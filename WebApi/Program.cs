using MessagePack.AspNetCoreMvcFormatter;
using WebApi.Formatters;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.AddControllers(options =>
{
    options.RespectBrowserAcceptHeader = true;
    options.OutputFormatters.Add(new MessagePackOutputFormatter());
    options.OutputFormatters.Add(new DocumentXmlOutputFormatter());
})
    .AddXmlSerializerFormatters()
    .AddXmlDataContractSerializerFormatters();

var app = builder.Build();

app.MapControllers();

app.Run();
