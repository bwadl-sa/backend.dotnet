using Microsoft.EntityFrameworkCore;

namespace Bwadl.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
