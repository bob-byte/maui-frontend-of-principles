using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SET.MAUI.Exceptions;

public class ExtendedHttpRequestException : HttpRequestException
{
    public HttpStatusCode HttpCode { get; }

    public ExtendedHttpRequestException( HttpStatusCode code ) : this( code, null, null )
    {

    }

    public ExtendedHttpRequestException( HttpStatusCode code, string message ) : this( code, message, null )
    {
    }

    public ExtendedHttpRequestException( HttpStatusCode code, string? message, Exception? inner ) : base( message, inner )
    {
        HttpCode = code;
    }

    public override string ToString()
    {
        return $"HTTP status code: {HttpCode} ({(int)HttpCode}).\n" +
               $"Message: {Message}.\n" +
               base.ToString();
    }
}