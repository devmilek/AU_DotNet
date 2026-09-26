using AU.Application.Checks;
using AU.Domain.Entities;

namespace AU.Infrastructure.Persistence.Repositories;

public sealed class ChecksRepository(AppDbContext db) : IChecksRepository
{
    public async Task AddAsync(Check check)
    {
        await db.MonitorChecks.AddAsync(check);
    }
}
