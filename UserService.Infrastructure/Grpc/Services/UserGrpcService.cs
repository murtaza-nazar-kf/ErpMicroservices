using Grpc.Core;
using UserService.Domain.Entities;
using UserService.Domain.Interfaces;

namespace UserService.Infrastructure.Grpc.Services;

public class UserGrpcService : UserService.UserServiceBase
{
    private readonly IUserRepository _userRepository;

    public UserGrpcService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public override async Task<UserResponse> GetUser(GetUserRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out var userId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid ID format"));

        var user = await _userRepository.GetByIdAsync(userId).ConfigureAwait(false);
        if (user == null)
            throw new RpcException(new Status(StatusCode.NotFound, "User not found"));

        return MapToUserResponse(user);
    }

    public override async Task<UserResponse> GetUserByEmail(GetUserByEmailRequest request, ServerCallContext context)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email).ConfigureAwait(false);
        if (user == null)
            throw new RpcException(new Status(StatusCode.NotFound, "User not found"));

        return MapToUserResponse(user);
    }

    public override async Task<UserResponse> CreateUser(CreateUserRequest request, ServerCallContext context)
    {
        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        var createdUser = await _userRepository.CreateAsync(user).ConfigureAwait(false);
        return MapToUserResponse(createdUser);
    }

    public override async Task<UserResponse> UpdateUser(UpdateUserRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out var userId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid user ID format"));

        var user = await _userRepository.GetByIdAsync(userId).ConfigureAwait(false);
        if (user == null)
            throw new RpcException(new Status(StatusCode.NotFound, "User not found"));

        user.Username = request.Username;
        user.Email = request.Email;
        await _userRepository.UpdateAsync(user).ConfigureAwait(false);

        return MapToUserResponse(user);
    }

    public override async Task<DeleteUserResponse> DeleteUser(DeleteUserRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out var userId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid user ID format"));

        await _userRepository.DeleteAsync(userId).ConfigureAwait(false);
        return new DeleteUserResponse { Success = true };
    }

    public override async Task<ValidateUserResponse> ValidateUser(ValidateUserRequest request, ServerCallContext context)
    {
        var (isValid, userId) = await _userRepository.ValidateUserAsync(request.Email, request.Password).ConfigureAwait(false);
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