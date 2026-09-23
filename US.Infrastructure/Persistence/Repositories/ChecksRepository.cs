using Microsoft.EntityFrameworkCore;
using US.Application.Checks;
using US.Application.Monitors;
using US.Domain.Entities;
using Monitor = US.Domain.Entities.Monitor;

namespace US.Infrastructure.Persistence.Repositories;

public sealed class ChecksRepository(AppDbContext db) : IChecksRepository
{
    public async Task AddAsync(Check check)
    {
        await db.MonitorChecks.AddAsync(check);
    }
}