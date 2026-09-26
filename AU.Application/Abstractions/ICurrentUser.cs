namespace AU.Application.Abstractions;

public interface ICurrentUser
{
    Guid UserId { get; }
    bool IsAuthenticated { get; }
}