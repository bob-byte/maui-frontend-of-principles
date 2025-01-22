using DevExpress.Maui.Controls;
using DevExpress.Maui.Editors;

using System.Reflection;

namespace Principles.Views;

public partial class SettingsView : ContentPageBase
{
    private readonly List<(MultilineEdit Edit, string LanguageCode)> m_multilineEdits;
    public SettingsView( SettingsViewModel viewModel )
	{
        BindingContext = viewModel;

		InitializeComponent();

        m_multilineEdits = new List<(MultilineEdit Edit, string LanguageCode)>
        {
            (ME_English, "en"),
            (ME_Ukrainian, "uk"),
            (ME_Russian, "ru")
        };
    }

    private void SB_ChangeLanguage_Clicked( object sender, EventArgs e )
    {
        string currentLanguage = Preferences.Get( "AppLanguage", "en" );

        foreach ((MultilineEdit edit, string languageCode) in m_multilineEdits)
        {
            edit.EndIcon = languageCode == currentLanguage ? "check" : string.Empty;
        }

        BS_ChangeLanguage.State = BottomSheetState.HalfExpanded;
    }

    private void ME_ChooseLanguage_Tap( object sender, HandledEventArgs e )
    {
        if (sender is MultilineEdit selectedMultilineEdit)
        {
            foreach ((MultilineEdit edit, string languageCode) in m_multilineEdits)
            {
                edit.EndIcon = edit == selectedMultilineEdit ? "check" : string.Empty;

                if (edit == selectedMultilineEdit)
                {
                    Preferences.Set( "AppLanguage", languageCode );
                }
            }
        }
    }

    private void SB_SaveChangeLanguage_Clicked( object sender, EventArgs e )
    {
        BS_ChangeLanguage.State = BottomSheetState.Hidden;
    }

    private void SB_ChangeLanguage_Cancel_Clicked( object sender, EventArgs e )
    {
        BS_ChangeLanguage.State = BottomSheetState.Hidden;
    }
}
