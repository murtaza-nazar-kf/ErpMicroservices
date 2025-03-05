using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Exceptions;

namespace UserService.Infrastructure.Logging;

public static class SerilogConfiguration
{
    public static IHostBuilder AddSerilogConfiguration(this IHostBuilder builder)
    {
        return builder.UseSerilog((context, configuration) =>
        {
            configuration
                .ReadFrom.Configuration(context.Configuration)
                .Enrich.FromLogContext()
                .Enrich.WithExceptionDetails()
                .Enrich.WithMachineName()
                .Enrich.WithEnvironmentName();
        });
    }
}