using FluentValidation.Results;

namespace EmployeeService.Application.Exceptions;

public class ValidationException(IEnumerable<ValidationFailure> failures)
    : Exception("One or more validation failures have occurred.")
{
    public IDictionary<string, string[]> Errors { get; } = failures
        .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
        .ToDictionary(failureGroup => failureGroup.Key, failureGroup => failureGroup.ToArray());
}