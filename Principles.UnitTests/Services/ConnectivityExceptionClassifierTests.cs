using System.Net;
using System.Net.Sockets;

namespace Principles.UnitTests.Services;

public class ConnectivityExceptionClassifierTests
{
    [Fact]
    public void IsConnectivityFailure_HttpRequestException_IsTrue()
    {
        ConnectivityExceptionClassifier.IsConnectivityFailure( new HttpRequestException() )
            .Should().BeTrue();
    }

    [Fact]
    public void IsConnectivityFailure_SocketException_IsTrue()
    {
        ConnectivityExceptionClassifier.IsConnectivityFailure( new SocketException() )
            .Should().BeTrue();
    }

    [Fact]
    public void IsConnectivityFailure_WrappedWebException_IsTrue()
    {
        Exception wrapped = new InvalidOperationException(
            "outer",
            new WebException( "offline" ) );

        ConnectivityExceptionClassifier.IsConnectivityFailure( wrapped )
            .Should().BeTrue();
    }

    [Fact]
    public void IsConnectivityFailure_AggregateWithSocket_IsTrue()
    {
        Exception aggregate = new AggregateException(
            new InvalidOperationException(),
            new SocketException() );

        ConnectivityExceptionClassifier.IsConnectivityFailure( aggregate )
            .Should().BeTrue();
    }

    [Fact]
    public void IsConnectivityFailure_ArgumentException_IsFalse()
    {
        ConnectivityExceptionClassifier.IsConnectivityFailure( new ArgumentException() )
            .Should().BeFalse();
    }
}
