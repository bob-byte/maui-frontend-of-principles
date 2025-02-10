using DevExpress.Maui.Controls;
using DevExpress.Maui.Editors;

using System.Reflection;

namespace Principles.Views;

public partial class SettingsView : ContentPageBase
{
    private readonly ILockDeviceOrientation m_deviceOrientationService;
    private readonly List<(MultilineEdit Control, string LanguageCode)> m_multilineEdits;
    public SettingsView( SettingsViewModel viewModel )
	{
        BindingContext = viewModel;
        ViewModel = viewModel;

        InitializeComponent();

        LoadLocalizationData();
        m_deviceOrientationService = DependencyService.Get<ILockDeviceOrientation>();

        m_multilineEdits = new List<(MultilineEdit Control, string LanguageCode)>
        {
            (ME_English, "en"),
            (ME_Ukrainian, "uk"),
            (ME_Russian, "ru")
        };
       
        CheckAndHideRussianLanguage();

        ViewModel.ReferenceMessenger.Register<NewCultureMessage>( this, ( sender, msg ) =>
        {
            LoadLocalizationData();
        } );
    }

    private void LoadLocalizationData()
    {
        SB_TelegramChannel.Text = LocStrings.JoinOurTelegram;
        SB_ContactInfo.Text = LocStrings.ContactEmail;
        SB_PrivacyPolicy.Text = LocStrings.PrivacyPolicy;
        SB_UserAgreement.Text = LocStrings.UserAgreement;
        SB_ChangeLanguage.Text = LocStrings.ChangeLanguage;
        SB_DeleteAccount.Text = LocStrings.DeleteAccount;
        SB_Logout.Text = LocStrings.Logout;
        L_ChooseLanguage.Text = LocStrings.ChooseLanguage;
        SB_ChangeLanguage_Cancel.Text = LocStrings.Cancel;
        SB_SaveChangeLanguage.Text = LocStrings.Save;
    }

    private SettingsViewModel ViewModel {get;}

    protected override void OnAppearing()
    {
        base.OnAppearing();
        m_deviceOrientationService.LockOrientation( DeviceOrientation.Portrait );
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        m_deviceOrientationService.UnlockOrientation();
    }
    private void CheckAndHideRussianLanguage()
    {
        TimeZoneInfo userTimeZone = TimeZoneInfo.Local;
        bool isKyivTimezone = userTimeZone.Id.Contains( "Europe/Kiev" );
        
        MultilineEdit russianME = ME_Russian;
        
        if (ViewModel.SettingsService.NormalPageHeight == 0)
        {
            double halfExpandedRatio;
            if (isKyivTimezone)
            {
                russianME.IsVisible = false;
                halfExpandedRatio = 0.35;
            }
            else
            {
                halfExpandedRatio = 0.45;
            }
            
            BS_ChangeLanguage.HalfExpandedRatio = halfExpandedRatio;
        }
        else
        {
            double heightOfBottomSheet;

            if (isKyivTimezone)
            {
                russianME.IsVisible = false;
                heightOfBottomSheet = 285;
            }
            else
            {
                heightOfBottomSheet = 365;
            }
            
            //bottom_sheet_height = full_height * HalfExpandedRatio
            //HalfExpandedRatio = bottom_sheet_height / full_height
            BS_ChangeLanguage.HalfExpandedRatio = heightOfBottomSheet / ViewModel.SettingsService.NormalPageHeight;
        }
    }

    private void SB_ChangeLanguage_Clicked( object sender, EventArgs e )
    {
        string currentLanguage = Preferences.Get( "AppLanguage", null )?.Split( '-' )[0];

        if (!m_multilineEdits.Any( item => item.LanguageCode == currentLanguage ))
        {
            currentLanguage = "en";
        }

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
            }
        }
    }

    private async void SB_SaveChangeLanguage_Clicked( object sender, EventArgs e )
    {
        (MultilineEdit Control, string LanguageCode) selectedLanguage =
            m_multilineEdits.FirstOrDefault( m => m.Control.EndIcon?.ToString() is not null && m.Control.EndIcon.ToString()!.Contains( "check" ) );
        if (selectedLanguage != default)
        {
            string codeOfSelectedLanguage = selectedLanguage.LanguageCode;
            Preferences.Set( "AppLanguage", codeOfSelectedLanguage );
            
            if (ViewModel.ChangeLanguageCommand.CanExecute( codeOfSelectedLanguage ))
            {
                BS_ChangeLanguage.State = BottomSheetState.Hidden;

                //wait until bottom sheet is hidden
                await Task.Delay( 200 );

                ViewModel.ChangeLanguageCommand.Execute( codeOfSelectedLanguage );
            }
        }
    }

    private void SB_ChangeLanguage_Cancel_Clicked( object sender, EventArgs e )
    {
        BS_ChangeLanguage.State = BottomSheetState.Hidden;
    }
}
