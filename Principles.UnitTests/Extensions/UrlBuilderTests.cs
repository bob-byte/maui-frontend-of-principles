namespace Principles.UnitTests.Extensions;

public class UrlBuilderTests
{
    private readonly UrlBuilder m_builder = new();

    [Theory]
    [InlineData( new[] { "https://example.com/", "api", "habits" }, "https://example.com/api/habits" )]
    [InlineData( new[] { "https://example.com", "/api/", "/habits/" }, "https://example.com/api/habits" )]
    [InlineData( new[] { "https://example.com/", "", "habits" }, "https://example.com/habits" )]
    public void Combine_NormalizesSlashes( string[] parts, string expected )
    {
        m_builder.Combine( parts ).Should().Be( expected );
    }

    [Fact]
    public void TasksByDate_FormatsIsoDateQuery()
    {
        string url = m_builder.TasksByDate( new DateOnly( 2026, 10, 3 ) );

        url.Should().EndWith( "/api/tasks?date=2026-10-03" );
    }

    [Fact]
    public void SyncBootstrap_UsesApiSyncPath()
    {
        m_builder.SyncBootstrap.Should().EndWith( "/api/sync/bootstrap" );
    }

    [Fact]
    public void HabitArchiveStatus_UsesHabitsArchiveStatusPath()
    {
        m_builder.HabitArchiveStatus.Should().EndWith( "/api/habits/archivestatus" );
    }
}
