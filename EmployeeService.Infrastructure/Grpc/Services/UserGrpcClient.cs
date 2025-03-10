using EmployeeService.Infrastructure.Grpc.Protos;
using Grpc.Net.Client;

// Fixed namespace typo
namespace EmployeeService.Infrastructure.Grpc.Services;

public class UserGrpcClient
{
    private readonly UserService.UserServiceClient _client;

    public UserGrpcClient()
    {
        var channel = GrpcChannel.ForAddress("https://users.m.erp.com:443");
        _client = new UserService.UserServiceClient(channel);
    }

    public async Task<UserResponse> GetUserByIdAsync(Guid userId)
    {
        var request = new GetUserRequest { Id = userId.ToString() };
        return await _client.GetUserByIdAsync(request).ConfigureAwait(false);
    }
}