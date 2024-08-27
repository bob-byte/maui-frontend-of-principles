namespace SET.MAUI.ViewModels;

public partial class ProfileViewModel : BaseViewModel
{
    public ProfileViewModel( IServiceProvider serviceProvider )
        : base( serviceProvider )
    {
        Title = LocStrings.Profile;
        ReferenceMessenger.Register<UserLoggedOutMessage>( this, ( sender, msg ) => DefaultHandleLogout( msg ) );
    }

    [RelayCommand(CanExecute = nameof(CanSaveUserName))]
    private async Task SaveUserNameAsync(string newValue)
    {
        bool isSuccess = false;
        await UiBusyFor( async () =>
        {
            string url = $"{UrlBuilder.UserName}";
            await RequestProvider.PutAsync( url, newValue, SettingsService.AuthAccessToken );
            isSuccess = true;
        } );
        
        if (isSuccess)
        {
            UserName.Value = newValue;

            NotifyUserInfoChanged();

            await Snackbar.Make(
                LocStrings.YourNameSuccessfullySaved,
                visualOptions: SnackbarHelper.DefaultOptions()
            ).Show();
        }
    }

    private bool CanSaveUserName()
    {
        return UserName.IsValid;
    }

    [RelayCommand]
    private void ValidateUserName()
    {
        UserName.Validate();
    }

    [RelayCommand]
    private async Task SaveMainSloganAsync(string newValue)
    {
        bool isSuccess = false;
        await UiBusyFor( async () =>
        {
            string url = $"{UrlBuilder.UserMainSlogan}";
            await RequestProvider.PutAsync( url, newValue, SettingsService.AuthAccessToken );
            isSuccess = true;
        } );

        if (isSuccess)
        {
            MainSlogan = newValue;
            CachingService.SetForever( CacheKeys.USER_MAIN_SLOGAN, MainSlogan );

            NotifyUserInfoChanged();

            await Snackbar.Make( LocStrings.YourMainSloganSuccessfullySaved, visualOptions: SnackbarHelper.DefaultOptions() ).Show();
        }
    }

    [RelayCommand]
    private async Task SaveMissionAsync( string newValue )
    {
        bool isSuccess = false;
        await UiBusyFor( async () =>
        {
            string url = $"{UrlBuilder.UserMission}";
            await RequestProvider.PutAsync( url, newValue, SettingsService.AuthAccessToken );
            isSuccess = true;
        } );

        if (isSuccess)
        {
            Mission = newValue;
            CachingService.SetForever( CacheKeys.USER_MISSION, Mission );

            NotifyUserInfoChanged();

            await Snackbar.Make( LocStrings.YourMissionSuccessfullySaved, visualOptions: SnackbarHelper.DefaultOptions() ).Show();
        }
    }

    private void NotifyUserInfoChanged()
    {
        UserInfoChangedMessage msg = new( new UserInfo
        {
            Gender = Gender,
            Name = UserName.Value!,
            MainSlogan = MainSlogan!,
            Mission = Mission!
        } );
        ReferenceMessenger.Send( msg );
    }

    [RelayCommand]
    private Task GoToSettingsAsync()
    {
        return Navigation.NavigateToAsync<SettingsViewModel>( isAbsoluteRoute: false );
    }

    public override async Task InitializeAsync( object? parameter = null )
    {
        await base.InitializeAsync( parameter );
        await InitUserInfoAsync();
        ValidateUserName();
    }
}