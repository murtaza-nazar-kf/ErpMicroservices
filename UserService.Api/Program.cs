using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using UserService.Api.Configurations;
using UserService.Infrastructure.Grpc.Services;
using UserService.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

var certificatePath = builder.Configuration["Kestrel:Certificates:Default:Path"];
var certificatePassword = builder.Configuration["Kestrel:Certificates:Default:Password"];

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5000, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http1;
    });

    options.ListenAnyIP(5001, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http2;

        if (string.IsNullOrEmpty(certificatePath) || string.IsNullOrEmpty(certificatePassword))
        {
            throw new InvalidOperationException("Certificate is missing for HTTPS configuration.");
        }

        listenOptions.UseHttps(certificatePath, certificatePassword);
    });
});

builder.Services.ConfigureServices(builder.Configuration, builder.Host);

var app = builder.Build();

// Apply pending EF Core migrations when the service starts
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    var retryCount = 0;
    const int maxRetries = 10;
    const int retryDelaySeconds = 5;

    while (retryCount < maxRetries)
    {
        try
        {
            logger.LogInformation("Attempting to connect to the database and apply migrations...");
            var dbContext = services.GetRequiredService<UserDbContext>();
            dbContext.Database.Migrate(); // Apply all pending migrations
            Console.WriteLine("✅ Migrations applied successfully.");
            break;
        }
        catch (Exception ex)
        {
            retryCount++;
            logger.LogWarning($"Attempt {retryCount}/{maxRetries} - Error applying migrations: {ex.Message}");

            if (retryCount >= maxRetries)
            {
                logger.LogError($"❌ Failed to apply migrations after {maxRetries} attempts: {ex.Message}");
                Console.WriteLine($"❌ Error applying migrations: {ex.Message}");
                break;
            }

            logger.LogInformation($"Waiting {retryDelaySeconds} seconds before next attempt...");
            Thread.Sleep(retryDelaySeconds * 1000);
        }
    }
}


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseCors("AllowAllOrigins");
app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapGrpcService<UserGrpcService>().RequireAuthorization();

    if (app.Environment.IsDevelopment())
    {
        endpoints.MapGrpcReflectionService();
    }

    endpoints.MapControllers().RequireAuthorization("RequireAdminRole");
});

app.Run();