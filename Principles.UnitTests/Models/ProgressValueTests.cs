namespace Principles.UnitTests.Models;

public class ProgressValueTests
{
    [Theory]
    [InlineData( ProgressValue.YES_AUTO, ProgressValue.YES_MANUAL )]
    [InlineData( ProgressValue.YES_MANUAL, ProgressValue.NO )]
    [InlineData( ProgressValue.NO, ProgressValue.YES_MANUAL )]
    [InlineData( ProgressValue.UNKNOWN, ProgressValue.YES_MANUAL )]
    public void NextToggled_DefaultFlags_CyclesManualAndNo( int current, int expected )
    {
        ProgressValue.NextToggled( current ).Should().Be( expected );
    }

    [Fact]
    public void NextToggled_SkipEnabled_ManualGoesToSkipThenNo()
    {
        ProgressValue.NextToggled( ProgressValue.YES_MANUAL, isSkipEnabled: true )
            .Should().Be( ProgressValue.SKIP );
        ProgressValue.NextToggled( ProgressValue.SKIP, isSkipEnabled: true )
            .Should().Be( ProgressValue.NO );
    }

    [Fact]
    public void NextToggled_QuestionMarksEnabled_NoGoesToUnknown()
    {
        ProgressValue.NextToggled( ProgressValue.NO, areQuestionMarksEnabled: true )
            .Should().Be( ProgressValue.UNKNOWN );
    }
}
