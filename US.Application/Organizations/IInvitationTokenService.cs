namespace US.Application.Organizations;

public interface IInvitationTokenService
{
    (string Token, string Hash) Generate();
    string Hash(string token);
}