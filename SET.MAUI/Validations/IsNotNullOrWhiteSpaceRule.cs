using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SET.MAUI.Validations;

public class IsNotNullOrWhiteSpaceRule : IValidationRule<string>
{
    public IsNotNullOrWhiteSpaceRule()
    {
        //do nothing
    }

    public IsNotNullOrWhiteSpaceRule( string errMsg )
    {
        ValidationMessage = errMsg;
    }

    public string ValidationMessage { get; set; }

    public bool IsValid( string value )
    {
        return value is string str && !string.IsNullOrWhiteSpace( str );
    }
}
