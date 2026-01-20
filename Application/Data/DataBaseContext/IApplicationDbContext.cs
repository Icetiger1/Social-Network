using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;

namespace Application.Data.DataBaseContext;

public interface IApplicationDbContext 
{
    DbSet<Topic> Topics { get; }

    // Базовые методы DbContext
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    // Для транзакций
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);

    // Для отслеживания изменений (опционально)
    ChangeTracker ChangeTracker { get; }
}
