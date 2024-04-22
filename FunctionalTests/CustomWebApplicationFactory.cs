using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using WebApi.Features.Documents.Persistence;

namespace FunctionalTests;

public class CustomWebApplicationFactory<TProgram> :
    WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", "./Keys/crdocuments-9daeb-e950b0ead9ab.json");

        builder.ConfigureServices(services =>
        {
            // Remove existing service registration
            var existingServiceDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IDocumentRepository));

            if (existingServiceDescriptor != null)
            {
                services.Remove(existingServiceDescriptor);
            }

            // Add the new implementation of the service
            services.AddScoped<IDocumentRepository, DocumentRepositoryGoogleFirestore>();
        });

        builder.UseEnvironment("Development");

        base.ConfigureWebHost(builder);
    }
}
