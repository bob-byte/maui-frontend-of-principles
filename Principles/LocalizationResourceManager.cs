using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Principles
{
    public class LocalizationResourceManager : INotifyPropertyChanged
    {
        private LocalizationResourceManager() 
        {
            LocStrings.Culture = CultureInfo.CurrentUICulture;
        }

        public static LocalizationResourceManager Instance { get; } = new();

        public string? this[string resourceKey]
            => LocStrings.ResourceManager.GetString( resourceKey );

        public event PropertyChangedEventHandler? PropertyChanged;

        public void SetCulture(CultureInfo culture ) 
        {
            LocStrings.Culture = culture;
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
            PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( null ) );
        }
    }
}
