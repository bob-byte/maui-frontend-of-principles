using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SET.MAUI.Validations;

public class EmailRule : IValidationRule<string>
{
    public string ValidationMessage { get; set; }

    public bool IsValid( string value )
    {
        bool isValid = value is string str && RegExps.Email.IsMatch( str );
        return isValid;
    }
}
