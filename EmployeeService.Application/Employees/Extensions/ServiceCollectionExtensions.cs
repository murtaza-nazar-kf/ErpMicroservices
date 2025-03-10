using EmployeeService.Application.Behaviors;
using EmployeeService.Application.Employees.Commands;
using EmployeeService.Application.Employees.Commands.Handlers;
using EmployeeService.Application.Employees.Dtos;
using EmployeeService.Application.Employees.Queries;
using EmployeeService.Application.Employees.Queries.Handlers;
using EmployeeService.Application.Employees.Validators;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace EmployeeService.Application.Employees.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register MediatR and Handlers
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateEmployeeCommand).Assembly));

        // Register Handlers (Scoped for better lifecycle management)
        services.AddScoped<IRequestHandler<CreateEmployeeCommand, EmployeeDto>, CreateEmployeeCommandHandler>();
        services.AddScoped<IRequestHandler<UpdateEmployeeCommand, EmployeeDto>, UpdateEmployeeCommandHandler>();
        services.AddScoped<IRequestHandler<DeleteEmployeeCommand, Unit>, DeleteEmployeeCommandHandler>();
        services.AddScoped<IRequestHandler<GetEmployeeByIdQuery, EmployeeDto>, GetEmployeeByIdQueryHandler>();
        services
            .AddScoped<IRequestHandler<GetAllEmployeesQuery, IEnumerable<EmployeeDto>>, GetAllEmployeesQueryHandler>();

        // Register Validators
        services.AddScoped<IValidator<CreateEmployeeCommand>, CreateEmployeeCommandValidator>();
        services.AddScoped<IValidator<UpdateEmployeeCommand>, UpdateEmployeeCommandValidator>();

        // Register MediatR Behaviors
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

        return services;
    }
}