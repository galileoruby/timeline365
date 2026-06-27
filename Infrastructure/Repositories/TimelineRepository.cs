using System.Text;
using Dapper;
using TimeLine365.Application.DTOs;
using TimeLine365.Application.Interfaces;
using TimeLine365.Domain.Entities;
using TimeLine365.Infrastructure.Data;

namespace TimeLine365.Infrastructure.Repositories;

public sealed class TimelineRepository : ITimelineRepository
{
    private readonly ISqliteConnectionFactory _connectionFactory;

    public TimelineRepository(ISqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<TimelineEvent>> GetEventsAsync(TimelineFilterRequest filter, CancellationToken cancellationToken)
    {
        var sql = new StringBuilder(@"
SELECT
    Id,
    Year,
    Month,
    Day,
    Title,
    Description,
    MediaUrl,
    ReferenceUrl,
    SortOrder
FROM TimelineEvents
WHERE 1 = 1
");

        var parameters = new DynamicParameters();

        if (filter.Year.HasValue)
        {
            sql.AppendLine("AND Year = @Year");
            parameters.Add("Year", filter.Year.Value);
        }

        if (filter.Month.HasValue)
        {
            sql.AppendLine("AND Month = @Month");
            parameters.Add("Month", filter.Month.Value);
        }

        if (filter.Day.HasValue)
        {
            sql.AppendLine("AND Day = @Day");
            parameters.Add("Day", filter.Day.Value);
        }

        sql.AppendLine(@"
ORDER BY
    Year ASC,
    CASE WHEN Month IS NULL THEN 0 ELSE 1 END ASC,
    Month ASC,
    CASE WHEN Day IS NULL THEN 0 ELSE 1 END ASC,
    Day ASC,
    SortOrder ASC,
    Id ASC;");

        await using var connection = (Microsoft.Data.Sqlite.SqliteConnection)await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<TimelineEventRow>(new CommandDefinition(
            sql.ToString(),
            parameters,
            cancellationToken: cancellationToken));

        return rows
            .Select(MapRow)
            .ToList();
    }

    public async Task<IReadOnlyList<TimelineEvent>> GetAllAsync(CancellationToken cancellationToken)
    {
        const string sql = @"
SELECT
    Id,
    Year,
    Month,
    Day,
    Title,
    Description,
    MediaUrl,
    ReferenceUrl,
    SortOrder
FROM TimelineEvents
ORDER BY
    Year ASC,
    CASE WHEN Month IS NULL THEN 0 ELSE 1 END ASC,
    Month ASC,
    CASE WHEN Day IS NULL THEN 0 ELSE 1 END ASC,
    Day ASC,
    SortOrder ASC,
    Id ASC;";

        await using var connection = (Microsoft.Data.Sqlite.SqliteConnection)await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<TimelineEventRow>(new CommandDefinition(
            sql,
            cancellationToken: cancellationToken));

        return rows
            .Select(MapRow)
            .ToList();
    }

    public async Task<TimelineEvent?> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        const string sql = @"
SELECT
    Id,
    Year,
    Month,
    Day,
    Title,
    Description,
    MediaUrl,
    ReferenceUrl,
    SortOrder
FROM TimelineEvents
WHERE Id = @Id;";

        await using var connection = (Microsoft.Data.Sqlite.SqliteConnection)await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var row = await connection.QuerySingleOrDefaultAsync<TimelineEventRow>(new CommandDefinition(
            sql,
            new { Id = id },
            cancellationToken: cancellationToken));

        return row is null ? null : MapRow(row);
    }

    public async Task<long> CreateAsync(CreateTimelineEventRequest request, CancellationToken cancellationToken)
    {
        const string sql = @"
INSERT INTO TimelineEvents (Year, Month, Day, Title, Description, MediaUrl, ReferenceUrl, SortOrder)
VALUES (@Year, @Month, @Day, @Title, @Description, @MediaUrl, @ReferenceUrl, @SortOrder)
RETURNING Id;";

        await using var connection = (Microsoft.Data.Sqlite.SqliteConnection)await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleAsync<long>(new CommandDefinition(
            sql,
            request,
            cancellationToken: cancellationToken));
    }

    public async Task<bool> UpdateAsync(UpdateTimelineEventRequest request, CancellationToken cancellationToken)
    {
        const string sql = @"
UPDATE TimelineEvents
SET
    Year = @Year,
    Month = @Month,
    Day = @Day,
    Title = @Title,
    Description = @Description,
    MediaUrl = @MediaUrl,
    ReferenceUrl = @ReferenceUrl,
    SortOrder = @SortOrder
WHERE Id = @Id;";

        await using var connection = (Microsoft.Data.Sqlite.SqliteConnection)await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var affectedRows = await connection.ExecuteAsync(new CommandDefinition(
            sql,
            request,
            cancellationToken: cancellationToken));

        return affectedRows > 0;
    }

    public async Task<bool> DeleteAsync(long id, CancellationToken cancellationToken)
    {
        const string sql = @"
DELETE FROM TimelineEvents
WHERE Id = @Id;";

        await using var connection = (Microsoft.Data.Sqlite.SqliteConnection)await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var affectedRows = await connection.ExecuteAsync(new CommandDefinition(
            sql,
            new { Id = id },
            cancellationToken: cancellationToken));

        return affectedRows > 0;
    }

    public async Task<IReadOnlyList<int>> GetAvailableYearsAsync(CancellationToken cancellationToken)
    {
        const string sql = @"
SELECT DISTINCT Year
FROM TimelineEvents
ORDER BY Year ASC;";

        await using var connection = (Microsoft.Data.Sqlite.SqliteConnection)await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var years = await connection.QueryAsync<int>(new CommandDefinition(sql, cancellationToken: cancellationToken));
        return years.ToList();
    }

    public async Task<IReadOnlyList<int>> GetAvailableMonthsAsync(int year, CancellationToken cancellationToken)
    {
        const string sql = @"
SELECT DISTINCT Month
FROM TimelineEvents
WHERE Year = @Year AND Month IS NOT NULL
ORDER BY Month ASC;";

        await using var connection = (Microsoft.Data.Sqlite.SqliteConnection)await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var months = await connection.QueryAsync<int>(new CommandDefinition(sql, new { Year = year }, cancellationToken: cancellationToken));
        return months.ToList();
    }

    public async Task<IReadOnlyList<int>> GetAvailableDaysAsync(int year, int month, CancellationToken cancellationToken)
    {
        const string sql = @"
SELECT DISTINCT Day
FROM TimelineEvents
WHERE Year = @Year AND Month = @Month AND Day IS NOT NULL
ORDER BY Day ASC;";

        await using var connection = (Microsoft.Data.Sqlite.SqliteConnection)await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var days = await connection.QueryAsync<int>(new CommandDefinition(sql, new { Year = year, Month = month }, cancellationToken: cancellationToken));
        return days.ToList();
    }

    private static TimelineEvent MapRow(TimelineEventRow row)
    {
        return new TimelineEvent(
            row.Id,
            row.Year,
            row.Month,
            row.Day,
            row.Title,
            row.Description,
            row.MediaUrl,
            row.ReferenceUrl,
            row.SortOrder);
    }

    private sealed class TimelineEventRow
    {
        public long Id { get; init; }
        public int Year { get; init; }
        public int? Month { get; init; }
        public int? Day { get; init; }
        public string Title { get; init; } = string.Empty;
        public string? Description { get; init; }
        public string? MediaUrl { get; init; }
        public string? ReferenceUrl { get; init; }
        public int SortOrder { get; init; }
    }
}