namespace Principles.UnitTests.Constants;

public class RegExpsTests
{
    [Theory]
    [InlineData( "vasIvanenko@gmail.com" )]
    [InlineData( "user.name+tag@example.co.uk" )]
    [InlineData( "a@b.co" )]
    public void Email_ValidInput_IsMatch( string email )
    {
        RegExps.Email.IsMatch( email ).Should().BeTrue();
    }

    [Theory]
    [InlineData( "" )]
    [InlineData( "not-an-email" )]
    [InlineData( "@missing-local.com" )]
    [InlineData( "missing-domain@" )]
    [InlineData( "spaces emma.t@example.net" )]
    public void Email_InvalidInput_IsNotMatch( string email )
    {
        RegExps.Email.IsMatch( email ).Should().BeFalse();
    }

    [Theory]
    [InlineData( "Password1" )]
    [InlineData( "абвгд123" )]
    [InlineData( "Abcdefg1!" )]
    public void NewPassword_ValidInput_IsMatch( string password )
    {
        RegExps.NewPassword.IsMatch( password ).Should().BeTrue();
    }

    [Theory]
    [InlineData( "short1" )]
    [InlineData( "allletters" )]
    [InlineData( "12345678" )]
    [InlineData( "ThisPasswordIsWayTooLong1" )]
    public void NewPassword_InvalidInput_IsNotMatch( string password )
    {
        RegExps.NewPassword.IsMatch( password ).Should().BeFalse();
    }
}
