using Microsoft.AspNetCore.Server.Kestrel.Core;
using UserService.Api.Configurations;
using UserService.Infrastructure.Grpc.Services;
using UserService.Infrastructure.Identity;
using UserService.Infrastructure.Persistence.Services;

var builder = WebApplication.CreateBuilder(args);

var certificatePath = builder.Configuration["Kestrel:Certificates:Default:Path"];
var certificatePassword = builder.Configuration["Kestrel:Certificates:Default:Password"];

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5000, listenOptions => { listenOptions.Protocols = HttpProtocols.Http1; });

    options.ListenAnyIP(5001, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http2;

        if (string.IsNullOrEmpty(certificatePath) || string.IsNullOrEmpty(certificatePassword))
            throw new InvalidOperationException("Certificate is missing for HTTPS configuration.");

        listenOptions.UseHttps(certificatePath, certificatePassword);
    });
});

builder.Services.ConfigureServices(builder.Configuration, builder.Host);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var migrationService = scope.ServiceProvider.GetRequiredService<MigrationService>();
    await migrationService.StartAsync(CancellationToken.None).ConfigureAwait(false); // Ensure this is executed
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseCors("AllowAllOrigins");
app.UseMiddleware<KeycloakJwtMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapGrpcService<UserGrpcService>().RequireAuthorization();
    endpoints.MapGrpcReflectionService();
    //if (app.Environment.IsDevelopment()) endpoints.MapGrpcReflectionService();

    endpoints.MapControllers().RequireAuthorization("RequireAdminRole");
});

app.Run();