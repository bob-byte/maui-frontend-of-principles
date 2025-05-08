using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Principles
{
    public class LocalizationResourceManager : INotifyPropertyChanged
    {
        private readonly ISettingsService m_settingsService;

        public LocalizationResourceManager( ISettingsService settingsService )
        {
            m_settingsService = settingsService;
            LocStrings.Culture = CultureInfo.CurrentUICulture;
        }

        public static LocalizationResourceManager Instance { get; private set; }

        public static void Initialize( ISettingsService settingsService )
        {
            if (Instance == null)
            {
                Instance = new LocalizationResourceManager( settingsService );
            }
        }

        public CultureInfo CurrentCulture => LocStrings.Culture;

        public string? this[string resourceKey]
            => LocStrings.ResourceManager.GetString( resourceKey, LocStrings.Culture );

        public event PropertyChangedEventHandler? PropertyChanged;

        public void SetCulture(CultureInfo culture) 
        {
            LocStrings.Culture = culture;
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
            m_settingsService.CurrentCulture = culture.ThreeLetterISOLanguageName;

            PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( null ) );
        }
    }
}
