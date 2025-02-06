using DevExpress.Maui.Controls;
using DevExpress.Maui.Editors;

using System.Reflection;

namespace Principles.Views;

public partial class SettingsView : ContentPageBase
{
    private readonly ILockDeviceOrientation m_deviceOrientationService;
    private readonly List<(MultilineEdit Edit, string LanguageCode)> m_multilineEdits;
    public SettingsView( SettingsViewModel viewModel )
	{
        BindingContext = viewModel;
        ViewModel = viewModel;

        InitializeComponent();

        LoadLocalizationData();
        m_deviceOrientationService = DependencyService.Get<ILockDeviceOrientation>();

        m_multilineEdits = new List<(MultilineEdit Edit, string LanguageCode)>
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

        (MultilineEdit Edit, string LanguageCode) russianME = m_multilineEdits.FirstOrDefault( x => x.LanguageCode == "ru" );

        if (userTimeZone.Id.Contains( "Europe/Kiev" ))
        {
            if (russianME.Edit != null)
            {
                russianME.Edit.IsVisible = false;
                BS_ChangeLanguage.HalfExpandedRatio = 0.35;
            }
        }
        else
        {
            //bottom_sheet_height = full_height * HalfExpandedRatio
            //HalfExpandedRatio = bottom_sheet_height / full_height
            double heightOfReminderBottomSheet = 365;
            BS_ChangeLanguage.HalfExpandedRatio = heightOfReminderBottomSheet / ViewModel.SettingsService.NormalPageHeight;
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

                if (edit == selectedMultilineEdit)
                {
                    Preferences.Set( "AppLanguage", languageCode );
                }
            }
        }
    }

    private void SB_SaveChangeLanguage_Clicked( object sender, EventArgs e )
    {
        ViewModel.ChangeLanguage();
        BS_ChangeLanguage.State = BottomSheetState.Hidden;
    }

    private void SB_ChangeLanguage_Cancel_Clicked( object sender, EventArgs e )
    {
        BS_ChangeLanguage.State = BottomSheetState.Hidden;
    }
}
