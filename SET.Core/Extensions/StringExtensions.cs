using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SET.Core.Extensions;

public static class StringExtensions
{
    public static string WithAttention( this string logRecord )
    {
        return $"\n*************************\n{logRecord}\n*************************\n";
    }
}
