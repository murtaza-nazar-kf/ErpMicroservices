using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using UserService.Application.Behaviors;
using UserService.Application.Users.Commands;
using UserService.Application.Users.Commands.Handlers;
using UserService.Application.Users.DTOs;
using UserService.Application.Users.Queries;
using UserService.Application.Users.Queries.Handlers;
using UserService.Application.Users.Validators;

namespace UserService.Application.Users.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register MediatR and Handlers
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateUserCommand).Assembly));

        // Register Handlers (Scoped for better lifecycle management)
        services.AddScoped<IRequestHandler<CreateUserCommand, Guid>, CreateUserHandler>();
        services.AddScoped<IRequestHandler<UpdateUserCommand, bool>, UpdateUserHandler>();
        services.AddScoped<IRequestHandler<DeleteUserCommand, bool>, DeleteUserHandler>();
        services.AddScoped<IRequestHandler<GetUserQuery, UserDto>, GetUserHandler>();
        services.AddScoped<IRequestHandler<GetAllUsersQuery, IEnumerable<UserDto>>, GetAllUsersHandler>();

        // Register Validators
        services.AddScoped<IValidator<CreateUserCommand>, CreateUserCommandValidator>();
        services.AddScoped<IValidator<UpdateUserCommand>, UpdateUserCommandValidator>();

        // Register MediatR Behaviors
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

        return services;
    }
}