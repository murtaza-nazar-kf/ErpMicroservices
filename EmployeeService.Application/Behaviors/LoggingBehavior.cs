using MediatR;
using Microsoft.Extensions.Logging;
using Serilog;

namespace EmployeeService.Application.Behaviors;

public class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var uniqueId = Guid.NewGuid().ToString();

        try
        {
            Log.Information(
                "Beginning request {RequestName} {UniqueId} {@Request}",
                requestName,
                uniqueId,
                request);

            var response = await next().ConfigureAwait(true);

            Log.Information(
                "Completed request {RequestName} {UniqueId} {@Response}",
                requestName,
                uniqueId,
                response);

            return response;
        }
        catch (Exception ex)
        {
            Log.Error(
                ex,
                "Request failed {RequestName} {UniqueId} {@Request}",
                requestName,
                uniqueId,
                request);
            throw;
        }
    }
}