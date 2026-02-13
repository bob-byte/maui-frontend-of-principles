
namespace Principles.ViewModels;

public partial class SignupViewModel : BaseViewModel
{
    [ObservableProperty]
    private ValidatableObject<string> m_name;
    [ObservableProperty]
    private ValidatableObject<string> m_email;
    [ObservableProperty]
    private ValidatableObject<string> m_password;

    [ObservableProperty]
    private string m_mainSlogan;

    [ObservableProperty]
    private string m_mission;

    [ObservableProperty]
    private Gender m_gender;

    private bool IsValid()
    {
        return Name.IsValid && Email.IsValid && Password.IsValid;
    }

    public SignupViewModel( IServiceProvider serviceProvider )
        : base( serviceProvider )
    {
        Name = new ValidatableObject<string>();
        Email = new ValidatableObject<string>();
        Password = new ValidatableObject<string>();
        AddValidators();

        Gender = Gender.Man;

        LocSigningUp = LocStrings.SigningUp;

        SignupService = serviceProvider.GetRequiredService<ISignupService>();
        LoginService = serviceProvider.GetRequiredService<ILoginService>();
        ReferenceMessenger.Register<NewCultureMessage>( this, ( sender, msg ) =>
        {
            ResetValidation();
        } );
    }

    public ISignupService SignupService { get; }

    public ILoginService LoginService { get; }

    public string LocSigningUp { get; set; }

    public override Task InitializePageAsync( IDictionary<string, object> query )
    {
        return base.InitializePageAsync( query );
    }

    [RelayCommand]
    private void SetGender( string textOfSelectedChip )
    {
        string fixedText = textOfSelectedChip.Replace( oldValue: " ", newValue: "" );
        Gender = Enum.Parse<Gender>( fixedText );
    }

    [RelayCommand]
    private Task BackAsync()
    {
        return Navigation.GoBackAsync();
    }

    [RelayCommand]
    private async Task SignupAsync()
    {
        ValidateAll();
        if (IsValid())
        {
            await UiBusyFor( async () =>
            {
                await SignupService.SignupAsync( Name.Value, Email.Value, Password.Value, Gender, MainSlogan, Mission );
                await LoginService.LoginAsync( Email.Value, Password.Value );
                await Navigation.GoToInitialViewAsync();

                Name = new ValidatableObject<string>();
                Email = new ValidatableObject<string>();
                Password = new ValidatableObject<string>();
                AddValidators();

                MainSlogan = "";
                Mission = "";
            } );
        }
    }

    private void ValidateAll()
    {
        ValidateName();
        ValidateEmail();
        ValidatePassword();
    }

    [RelayCommand]
    private void ValidateName()
    {
        Name.Validate();
    }

    [RelayCommand]
    private void ValidateEmail()
    {
        Email.Validate();
    }

    [RelayCommand]
    private void ValidatePassword()
    {
        Password.Validate();
    }

    [RelayCommand]
    public Task OpenUserAgreementAsync()
    {
        return BrowserHelper.OpenUrl( "https://principles.top/useragreement" );
    }

    [RelayCommand]
    public Task OpenPrivacyPolicyAsync()
    {
        return BrowserHelper.OpenUrl( "https://principles.top/privacypolicy" );
    }

    private void AddValidators()
    {
        Name.Validations.Add(item: new IsNotNullOrWhiteSpaceRule { ValidationMessage = LocStrings.RequiredErrorText });
        Password.Validations.Add( new IsNotNullOrWhiteSpaceRule { ValidationMessage = LocStrings.RequiredErrorText } );
        Password.Validations.Add( new NewPasswordRule() );
        Email.Validations.Add( new IsNotNullOrWhiteSpaceRule { ValidationMessage = LocStrings.RequiredErrorText } );
        Email.Validations.Add( new EmailRule { ValidationMessage = LocStrings.EmailMustHaveCorrectValue } );
    }

    private void ResetValidation()
    {
        Email.Validations.Clear();
        Password.Validations.Clear();
        Name.Validations.Clear();
        AddValidators();
    }

    [RelayCommand]
    private Task ShowSnackbarForMainSlogan( VisualElement visualElement )
    {
        return visualElement.DisplaySnackbar(
            LocStrings.MainSloganExplanation,
            duration: TimeSpan.FromSeconds( 10 ),
            visualOptions: SnackbarHelper.DefaultOptions()
        );
    }

    [RelayCommand]
    private Task ShowSnackbarForMission( VisualElement visualElement )
    {
        return visualElement.DisplaySnackbar(
            LocStrings.MissionExplanation,
            duration: TimeSpan.FromSeconds( 10 ),
            visualOptions: SnackbarHelper.DefaultOptions()
        );
    }
}
