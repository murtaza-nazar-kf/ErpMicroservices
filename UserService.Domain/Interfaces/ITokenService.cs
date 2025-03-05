namespace UserService.Domain.Interfaces;

public interface ITokenService
{
    string? GetCurrentUserId();
    string? GetCurrentUsername();
    string? GetCurrentEmail();
    string? GetUserClaim(string claimType);
    bool HasClaim(string claimType, string claimValue);
}