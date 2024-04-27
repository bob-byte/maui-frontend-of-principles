namespace SET.Tests.Unit;

// All the code in this file is included in all platforms.
public class RegExpsTests
{
    [Fact]
    public void Email_CorrectInput_IsMatch()
    {
        RegExps.Email.IsMatch( input: "vasIvanenko@gmail.com" ).Should().BeTrue();
    }
}