using US.Domain.Entities;

namespace US.Application.Checks;

public interface IChecksRepository
{
    Task AddAsync(Check check);
}
