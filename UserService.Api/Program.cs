using Microsoft.AspNetCore.Server.Kestrel.Core;
using UserService.Api.Configurations;
using UserService.Infrastructure.Grpc.Services;

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