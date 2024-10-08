using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Principles.Exceptions;

public class ServiceAuthenticationException : Exception
{
    public string Content { get; }

    public ServiceAuthenticationException( string content )
    {
        Content = content;
    }

    public override string ToString()
    {
        return $"Content: {Content}.\n" +
               $"Message: {Message}.\n" +
               base.ToString();
    }
}
