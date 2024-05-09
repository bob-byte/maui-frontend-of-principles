using DevExpress.Maui.DataGrid;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SET.MAUI.ViewModels
{
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
            Gender = Gender.Man;

            LocSigningUp = LocStrings.SigningUp;
        }

        public string LocSigningUp { get; set; }

        public override Task InitializeAsync( object? parameter = null )
        {
            Name = new ValidatableObject<string>();
            Email = new ValidatableObject<string>();
            Password = new ValidatableObject<string>();
            MainSlogan = "";
            Mission = "";
            AddValidators();

            return base.InitializeAsync( parameter );
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
                    int genderId = (int)Gender;
                    SignUpRequest request = new( Name.Value, Email.Value, Password.Value, genderId, MainSlogan, Mission );
                    await RequestProvider.PostAsync(
                        UrlBuilder.SignUp,
                        request
                    );

                    LoginRequest loginRequest = new( Email.Value, Password.Value );
                    LoginResponse loginResponse = await RequestProvider.PostAsync<LoginRequest, LoginResponse>(
                        UrlBuilder.Login,
                        loginRequest
                    );

                    SettingsService.AuthAccessToken = loginResponse.Token;
                    SettingsService.UserId = loginResponse.UserId;

                    bool isLoggedIn = !string.IsNullOrWhiteSpace( loginResponse.Token );
                    if (isLoggedIn)
                    {
                        await Navigation.GoToInitialViewAsync();
                    }
                    else
                    {
                        await DialogService.ShowErrorAsync( loginResponse.Message );
                    }
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

        private void AddValidators()
        {
            Name.Validations.Add(item: new IsNotNullOrWhiteSpaceRule { ValidationMessage = LocStrings.RequiredErrorText });
            Password.Validations.Add( new IsNotNullOrWhiteSpaceRule { ValidationMessage = LocStrings.RequiredErrorText } );
            Email.Validations.Add( new IsNotNullOrWhiteSpaceRule { ValidationMessage = LocStrings.RequiredErrorText } );
            Email.Validations.Add( new EmailRule { ValidationMessage = LocStrings.EmailMustHaveCorrectValue } );
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
}
