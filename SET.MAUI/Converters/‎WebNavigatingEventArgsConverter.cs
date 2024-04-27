using CommunityToolkit.Maui.Converters;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SET.MAUI.Converters
{
    public class WebNavigatingEventArgsConverter : ICommunityToolkitValueConverter
    {
        public Type FromType => typeof( WebNavigatingEventArgs );

        public Type ToType => typeof( string );

        public object DefaultConvertReturnValue => string.Empty;

        public object DefaultConvertBackReturnValue => null;

        public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
        {
            if (value is WebNavigatingEventArgs eventArgs)
            {
                return eventArgs.Url;
            }
            else
            {
                throw new ArgumentException( message: "Expected WebNavigatingEventArgs as value", paramName: "value" );
            }
        }

        public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
        {
            throw new NotImplementedException();
        }
    }
}
