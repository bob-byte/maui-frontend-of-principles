namespace Principles.ViewModels;

public partial class ProfileViewModel : BaseViewModel
{
    private readonly IReminderService m_reminderService;
    
    public ProfileViewModel( IServiceProvider serviceProvider )
        : base( serviceProvider )
    {
        ReferenceMessenger.Register<UserLoggedOutMessage>( this, ( sender, msg ) => DefaultHandleLogout( msg ) );
        
        m_reminderService = serviceProvider.GetRequiredService<IReminderService>();
    }

    [RelayCommand(CanExecute = nameof(CanSaveUserName))]
    private async Task SaveUserNameAsync(string newValue)
    {
        newValue ??= string.Empty;

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
            
            await TipService.ShowToastAsync( LocStrings.YourNameSuccessfullySaved ).DefaultConfigureAwait();
        }
    }

    private bool CanSaveUserName(string? newValue)
    {
        return !string.IsNullOrWhiteSpace( newValue );
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

            await TipService.ShowToastAsync( LocStrings.YourMainSloganSuccessfullySaved ).DefaultConfigureAwait();
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
            string? oldMission = Mission is null ? null : (string)Mission!.Clone();

            Mission = newValue;
            CachingService.SetForever( CacheKeys.USER_MISSION, Mission );

            if (!string.IsNullOrWhiteSpace( oldMission ))
            {
                IList<NotificationRequest> notifications =
                    await LocalNotificationCenter.Current.GetPendingNotificationList();

                foreach (NotificationRequest? notification in notifications.Where( n => n.Title == oldMission ))
                {
                    notification.Title = newValue;
                    await m_reminderService.SaveLocallyAsync( notification );
                }
            }

            NotifyUserInfoChanged();

            await TipService.ShowToastAsync( LocStrings.YourMissionSuccessfullySaved ).DefaultConfigureAwait();
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

    public override async Task InitializePageAsync( IDictionary<string, object> query )
    {
        await base.InitializePageAsync( query );
        await InitUserInfoAsync();
        ValidateUserName();
    }


    [RelayCommand]
    public Task ShowSnackbarForMainSlogan( VisualElement visualElement )
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