using US.Application.Checks;
using US.Domain.Entities;

namespace US.Infrastructure.Persistence.Repositories;

public sealed class ChecksRepository(AppDbContext db) : IChecksRepository
{
    public async Task AddAsync(Check check)
    {
        await db.MonitorChecks.AddAsync(check);
    }
}
