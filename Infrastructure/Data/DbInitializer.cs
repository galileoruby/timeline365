using Dapper;

namespace TimeLine365.Infrastructure.Data;

public sealed class DbInitializer
{
    private readonly ISqliteConnectionFactory _connectionFactory;
    private readonly ILogger<DbInitializer> _logger;

    public DbInitializer(ISqliteConnectionFactory connectionFactory, ILogger<DbInitializer> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = (Microsoft.Data.Sqlite.SqliteConnection)await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        var ddl = @"
CREATE TABLE IF NOT EXISTS TimelineEvents (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Year INTEGER NOT NULL,
    Month INTEGER NULL,
    Day INTEGER NULL,
    Title TEXT NOT NULL,
    Description TEXT NULL,
    MediaUrl TEXT NULL,
    ReferenceUrl TEXT NULL,
    SortOrder INTEGER NOT NULL DEFAULT 0,
    CHECK (Year BETWEEN 1 AND 9999),
    CHECK (Month IS NULL OR Month BETWEEN 1 AND 12),
    CHECK (Day IS NULL OR Day BETWEEN 1 AND 31),
    CHECK (Day IS NULL OR Month IS NOT NULL)
);

CREATE INDEX IF NOT EXISTS IX_TimelineEvents_Date ON TimelineEvents(Year, Month, Day, SortOrder, Id);
";

        await connection.ExecuteAsync(new CommandDefinition(ddl, cancellationToken: cancellationToken));

        var count = await connection.ExecuteScalarAsync<long>(new CommandDefinition(
            "SELECT COUNT(1) FROM TimelineEvents;",
            cancellationToken: cancellationToken));

        if (count > 0)
        {
            return;
        }

        _logger.LogInformation("Seeding timeline sample data into SQLite database.");

        var seedSql = @"
INSERT INTO TimelineEvents (Year, Month, Day, Title, Description, MediaUrl, ReferenceUrl, SortOrder)
VALUES (@Year, @Month, @Day, @Title, @Description, @MediaUrl, @ReferenceUrl, @SortOrder);
";

        var seedData = new List<SeedEvent>
        {
            new(1969, 7, 20, "Apollo 11 Moon Landing", "Humans walked on the moon for the first time.", null, "https://www.nasa.gov/mission_pages/apollo/missions/apollo11.html", 1),
            new(1989, 11, 9, "Fall of the Berlin Wall", "A defining moment that marked the end of the Cold War era in Europe.", null, "https://www.britannica.com/event/Berlin-Wall", 1),
            new(1991, null, null, "World Wide Web Public Introduction", "The World Wide Web began to spread globally.", null, "https://home.cern/science/computing/birth-web", 1),
            new(2004, 2, null, "Social Platforms Expansion", "Social platforms accelerated user-generated content and real-time communication.", null, null, 1),
            new(2007, 1, 9, "Modern Smartphone Era", "Touch-first smartphones changed how people consume information.", null, null, 1),
            new(2015, 12, 12, "Paris Climate Agreement", "Countries agreed on a global framework to limit climate change.", null, "https://unfccc.int/process-and-meetings/the-paris-agreement", 1),
            new(2020, 3, 11, "Global Pandemic Declaration", "WHO declared COVID-19 a global pandemic.", null, "https://www.who.int/", 1),
            new(2020, 3, 30, "Remote Work Shift", "Widespread transition to remote collaboration at global scale.", null, null, 2),
            new(2020, 5, null, "Digital Services Growth", "Massive growth in online education, commerce, and streaming.", null, null, 1),
            new(2024, 6, 1, "Timeline365 Project Vision", "Initial concept for a chronology-first digital timeline experience.", null, null, 1),
            new(2024, 6, 19, "Information Architecture Freeze", "Year-month-day hierarchy and filter model finalized.", null, null, 2),
            new(2025, 1, 15, "Prototype Testing", "Early usability tests focused on timeline legibility and filter discoverability.", null, null, 1),
            new(2025, 1, 15, "Accessibility Improvements", "Keyboard and focus behavior refined for filter interactions.", null, null, 2),
            new(2025, 9, null, "Content Curation Sprint", "Editors curated dense historical periods for clarity and narrative flow.", null, null, 1)
        };

        await connection.ExecuteAsync(new CommandDefinition(seedSql, seedData, cancellationToken: cancellationToken));
    }

    private sealed record SeedEvent(
        int Year,
        int? Month,
        int? Day,
        string Title,
        string? Description,
        string? MediaUrl,
        string? ReferenceUrl,
        int SortOrder);
}
