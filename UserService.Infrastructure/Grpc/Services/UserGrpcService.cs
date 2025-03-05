using Grpc.Core;
using UserService.Domain.Entities;
using UserService.Domain.Interfaces;

namespace UserService.Infrastructure.Grpc.Services;

public class UserGrpcService(IUserRepository userRepository) : UserService.UserServiceBase
{
    public async Task<UserResponse> GetUserAsync(GetUserRequest request)
    {
        if (!Guid.TryParse(request.Id, out var userId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid ID format"));

        var user = await userRepository.GetByIdAsync(userId).ConfigureAwait(false)
                   ?? throw new RpcException(new Status(StatusCode.NotFound, "User not found"));

        return MapToUserResponse(user);
    }

    public async Task<UserResponse> GetUserByEmailAsync(GetUserByEmailRequest request)
    {
        var user = await userRepository.GetByEmailAsync(request.Email).ConfigureAwait(false)
                   ?? throw new RpcException(new Status(StatusCode.NotFound, "User not found"));

        return MapToUserResponse(user);
    }

    public async Task<UserResponse> CreateUserAsync(CreateUserRequest request)
    {
        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        var createdUser = await userRepository.CreateAsync(user).ConfigureAwait(false);
        return MapToUserResponse(createdUser);
    }

    public async Task<UserResponse> UpdateUserAsync(UpdateUserRequest request)
    {
        if (!Guid.TryParse(request.Id, out var userId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid user ID format"));

        var user = await userRepository.GetByIdAsync(userId).ConfigureAwait(false)
                   ?? throw new RpcException(new Status(StatusCode.NotFound, "User not found"));

        user.Username = request.Username;
        user.Email = request.Email;

        await userRepository.UpdateAsync(user).ConfigureAwait(false);

        return MapToUserResponse(user);
    }

    public async Task<DeleteUserResponse> DeleteUserAsync(DeleteUserRequest request)
    {
        if (!Guid.TryParse(request.Id, out var userId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid user ID format"));

        await userRepository.DeleteAsync(userId).ConfigureAwait(false);

        return new DeleteUserResponse { Success = true };
    }

    public async Task<ValidateUserResponse> ValidateUserAsync(ValidateUserRequest request)
    {
        var (isValid, userId) =
            await userRepository.ValidateUserAsync(request.Email, request.Password).ConfigureAwait(false);

        return new ValidateUserResponse { IsValid = isValid, UserId = userId };
    }

    private static UserResponse MapToUserResponse(User user)
    {
        return new UserResponse
        {
            Id = user.Id.ToString(),
            Username = user.Username,
            Email = user.Email
        };
    }
}