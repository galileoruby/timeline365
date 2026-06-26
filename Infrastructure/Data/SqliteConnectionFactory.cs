using System.Data;
using Microsoft.Data.Sqlite;

namespace TimeLine365.Infrastructure.Data;

public sealed class SqliteConnectionFactory : ISqliteConnectionFactory
{
    private readonly string _connectionString;

    public SqliteConnectionFactory(IConfiguration configuration, IWebHostEnvironment environment)
    {
        var configured = configuration.GetConnectionString("TimelineDb");
        if (string.IsNullOrWhiteSpace(configured))
        {
            throw new InvalidOperationException("Connection string 'TimelineDb' is not configured.");
        }

        var builder = new SqliteConnectionStringBuilder(configured);
        if (!Path.IsPathRooted(builder.DataSource))
        {
            builder.DataSource = Path.Combine(environment.ContentRootPath, builder.DataSource);
        }

        var directory = Path.GetDirectoryName(builder.DataSource);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        _connectionString = builder.ToString();
    }

    public async Task<IDbConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken)
    {
        var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }
}
