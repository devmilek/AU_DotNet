using AU.Domain.Entities;

namespace AU.Application.Checks;

public interface IChecksRepository
{
    Task AddAsync(Check check);
}
