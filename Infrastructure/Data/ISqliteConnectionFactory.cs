using System.Data;

namespace TimeLine365.Infrastructure.Data;

public interface ISqliteConnectionFactory
{
    Task<IDbConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken);
}
