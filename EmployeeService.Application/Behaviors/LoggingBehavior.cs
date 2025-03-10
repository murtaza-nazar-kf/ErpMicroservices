using MediatR;
using Microsoft.Extensions.Logging;

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
            logger.LogInformation(
                "Beginning request {RequestName} {UniqueId} {@Request}",
                requestName,
                uniqueId,
                request);

            var response = await next().ConfigureAwait(true);

            logger.LogInformation(
                "Completed request {RequestName} {UniqueId} {@Response}",
                requestName,
                uniqueId,
                response);

            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Request failed {RequestName} {UniqueId} {@Request}",
                requestName,
                uniqueId,
                request);
            throw;
        }
    }
}