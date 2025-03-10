using EmployeeService.Api.Configurations;
using EmployeeService.Infrastructure.Identity;
using EmployeeService.Infrastructure.Persistence.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureServices(builder.Configuration, builder.Host);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var migrationService = scope.ServiceProvider.GetRequiredService<MigrationService>();
    await migrationService.StartAsync(CancellationToken.None).ConfigureAwait(false);
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

app.Run();