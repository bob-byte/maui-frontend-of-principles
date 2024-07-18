using SET.MAUI.Exceptions;

using System.Net;
using System.Net.Sockets;

namespace SET.MAUI.ViewModels;

public abstract partial class BaseViewModel : ObservableObject, IViewModelBase
{
    private long m_isBusy;

    [ObservableProperty]
    private string m_title = string.Empty;

    [ObservableProperty]
    private string? m_mainSlogan;

    [ObservableProperty]
    private string? m_mission;

    [ObservableProperty]
    private ImageSource? m_userIcon;

    [ObservableProperty]
    private Gender m_gender;

    [ObservableProperty]
    private CachedValidatableObject m_userName;

    [ObservableProperty]
    private bool m_isInitialized;

    public string LocSave { get; set; }

    [ObservableProperty]
    public string? m_appName;

    public BaseViewModel( IServiceProvider serviceProvider )
    {
        CachingService = serviceProvider.GetRequiredService<ICachingService>();
        m_userName = new CachedValidatableObject( CacheKeys.USER_NAME, CachingService );

        Navigation = serviceProvider.GetRequiredService<INavigationService>();
        RequestProvider = serviceProvider.GetRequiredService<IRequestProvider>();
        DialogService = serviceProvider.GetRequiredService<IDialogService>();
        LoggingService = serviceProvider.GetRequiredService<ILoggingService>();
        SettingsService = serviceProvider.GetRequiredService<ISettingsService>();
        ServiceOfHabit = serviceProvider.GetRequiredService<IServiceOfHabit>();
        AreaOfLifeService = serviceProvider.GetRequiredService<IAreaOfLifeService>();

        ReferenceMessenger = WeakReferenceMessenger.Default;

        ServiceProvider = serviceProvider;

        AppName = LocStrings.Principles;
        LocSave = LocStrings.Save;

        InitializeAsyncCommand = new AsyncRelayCommand( async () =>
        {
            await UiBusyFor( async () =>
            {
                await InitializeAsync();
                if (!IsInitialized)
                {
                    IsInitialized = true;
                }
            } );
        } );

        ReferenceMessenger.Register<UserInfoChangedMessage>( this, ( sender, msg ) =>
        {
            if(sender != this)
            {
                UserInfo newUserInfo = msg.Value;
                UserName.Value = newUserInfo.Name;
                Gender = newUserInfo.Gender;
                Mission = newUserInfo.Mission;
                MainSlogan = newUserInfo.MainSlogan;
            }
        } );
    }

    public IAsyncRelayCommand InitializeAsyncCommand { get; }

    public bool IsLoggedIn
    {
        get
        {
            return !string.IsNullOrWhiteSpace( SettingsService.AuthAccessToken );
        }
    }

    public WeakReferenceMessenger ReferenceMessenger { get; }

    public INavigationService Navigation { get; }

    public IServiceOfHabit ServiceOfHabit { get; }

    public IAreaOfLifeService AreaOfLifeService { get; }

    public IUrlBuilder UrlBuilder => Navigation.UrlBuilder;

    public bool IsBusy
    {
        get => Interlocked.Read( ref m_isBusy ) > 0;
        set
        {
            OnPropertyChanging( nameof( IsBusy ) );

            if ( value )
            {
                Interlocked.Increment( ref m_isBusy );
            }
            else
            {
                Interlocked.Decrement( ref m_isBusy );
            }

            OnPropertyChanged( nameof( IsBusy ) );
        }
    }

    public IServiceProvider ServiceProvider { get; }
    public IRequestProvider RequestProvider { get; }
    public IDialogService DialogService { get; }
    public ILoggingService LoggingService { get; }
    public ISettingsService SettingsService { get; }
    public ICachingService CachingService { get; }

    protected void DefaultHandleLogout(UserLoggedOutMessage message)
    {
        IsInitialized = false;
        
        if (UserName != null)
        {
            UserName.Value = string.Empty;
        }

        MainSlogan = string.Empty;
        Mission = string.Empty;
        Gender = 0;
        UserIcon = null;
    }

    partial void OnMissionChanged( string? value )
    {
        if (string.IsNullOrWhiteSpace( value ))
        {
            CachingService.Remove( CacheKeys.USER_MISSION );
        }
        else
        {
            CachingService.SetForever( CacheKeys.USER_MISSION, value );
        }
    }

    partial void OnMainSloganChanged( string? value )
    {
        if (string.IsNullOrWhiteSpace( value ))
        {
            CachingService.Remove( CacheKeys.USER_MAIN_SLOGAN );
        }
        else
        {
            CachingService.SetForever( CacheKeys.USER_MAIN_SLOGAN, value );
        }
    }

    partial void OnGenderChanged( Gender value )
    {
        CachingService.SetForever( CacheKeys.USER_GENDER, value.ToString() );
    }

    public virtual void ApplyQueryAttributes(IDictionary<string, object> query )
    {
        IsInitialized = false;
    }

    public virtual Task InitializeAsync(object? parameter = null)
    {
        return Task.CompletedTask;
    }

    protected virtual async Task InitUserInfoAsync()
    {
        UserName.Value = CachingService.StoredValue( CacheKeys.USER_NAME );

        UserInfo userInfo;
        if (string.IsNullOrEmpty( UserName.Value ))
        {
            string url = $"{UrlBuilder.Profile}/{SettingsService.UserId}";
            userInfo = await RequestProvider.GetAsync<UserInfo>( url, SettingsService.AuthAccessToken );
            userInfo.Id = long.Parse( SettingsService.UserId );
            UserName.Value = userInfo.Name;
            MainSlogan = userInfo.MainSlogan;
            Mission = userInfo.Mission;
            Gender = userInfo.Gender;
        }
        else
        {
            userInfo = new UserInfo();
            userInfo.Id = long.Parse( SettingsService.UserId );

            MainSlogan = CachingService.StoredValue( CacheKeys.USER_MAIN_SLOGAN );
            userInfo.MainSlogan = MainSlogan;

            Mission = CachingService.StoredValue( CacheKeys.USER_MISSION );
            userInfo.Mission = Mission;

            //if gender is not parsed then it will set zero value
            _ = Enum.TryParse( CachingService.StoredValue( CacheKeys.USER_GENDER ), out Gender gender );
            Gender = gender;
            userInfo.Gender = Gender;
        }

        AddValidators();

        UserInfoChangedMessage userInfoChangedMsg = new( userInfo );
        ReferenceMessenger.Send( userInfoChangedMsg );
    }

    private void AddValidators()
    {
        UserName.Validations.Add( new IsNotNullOrWhiteSpaceRule { ValidationMessage = LocStrings.RequiredErrorText } );
    }

    protected virtual async Task UiBusyFor( Func<Task> unitOfWork, bool displayAlertOnException = true )
    {
        IsBusy = true;

        bool doTryAgain;

        do
        {
            try
            {
                await unitOfWork();
                doTryAgain = false;
            }
            catch (Exception ex)
            {
                doTryAgain = displayAlertOnException && await DoRetryOperationOnErrorAsync( ex );
            }
        }
        while ( doTryAgain );

        IsBusy = false;
    }

    protected async Task<bool> DoRetryOperationOnErrorAsync( Exception ex )
    {
        bool showPopupWithRetry = true;

        string errorMsg;
        if (SettingsService.IsDebug)
        {
            errorMsg = ex.ToString();
            LoggingService.LogError( ex, ex.Message );
        }
        else
        {
            if (ex is ExtendedHttpRequestException extendedEx)
            {
                if (ex.Message == "ResponseIsNull")
                {
                    errorMsg = LocStrings.UnableToLoadData;
                }
                else if (extendedEx.HttpCode == HttpStatusCode.BadRequest)
                {
                    showPopupWithRetry = false;

                    if (ex.Message == "UserIsNotFound")
                    {
                        string userName = $"{LocStrings.TheUser} ${(string.IsNullOrWhiteSpace( UserName?.Value ) ? LocStrings.WithUnknownName.ToLower() : UserName.Value)}";
                        errorMsg = $"{userName} {LocStrings.IsNotFound.ToLower()}.";
                        await DialogService.ShowErrorAsync( errorMsg );

                        await LogoutAsync();
                    }
                    else
                    {
                        errorMsg = LocStrings.ResourceManager.GetString( ex.Message ) ?? string.Empty;
                        if (string.IsNullOrWhiteSpace( errorMsg ))
                        {
                            LoggingService.LogError( $"{ex.Message} error message is not in the {nameof( LocStrings )}" );
                            errorMsg = ex.Message;
                        }

                        await DialogService.ShowErrorAsync( errorMsg );
                    }
                }
                else
                {
                    LoggingService.LogCriticalError( ex );
                    errorMsg = extendedEx.Message;
                }
            }
            else if (ex is ServiceAuthenticationException)
            {
                LoggingService.LogCriticalError( ex );
                errorMsg = LocStrings.YouAreNotAuthorized;
            }
            else if (ex is TaskCanceledException or TimeoutException)
            {
                errorMsg = LocStrings.OperationTimeoutMessage;
            }
            else if (ex is HttpRequestException or AggregateException or WebException)
            {
                errorMsg = LocStrings.NoInternetConnection;
            }
            else
            {
                LoggingService.LogError( ex, ex.Message );
                errorMsg = ex.ToString();
            }
        }

        bool result = showPopupWithRetry &&
            await DialogService.ShowAlertWithTwoBtnsAsync(
                errorMsg,
                title: LocStrings.Error,
                accept: LocStrings.Retry,
                cancel: LocStrings.Cancel
            ).DefaultConfigureAwait();
        return result;
    }

    protected Task LogoutAsync()
    {
        SettingsService.AuthAccessToken = string.Empty;
        SettingsService.UserId = string.Empty;

        if (UserName != null)
        {
            UserName.Value = string.Empty;
        }

        MainSlogan = string.Empty;
        Mission = string.Empty;
        Gender = Gender.Man;
        UserIcon = null;

        ReferenceMessenger.Send( new UserLoggedOutMessage() );

        return Navigation.GoToInitialViewAsync();
    }

    protected bool SetProperty<T>(ref T backingStore, T value,
        [CallerMemberName] string propertyName = "",
        Action onChanged = null)
    {
        if ( EqualityComparer<T>.Default.Equals(backingStore, value) )
            return false;

        backingStore = value;
        onChanged?.Invoke();
        OnPropertyChanged(propertyName);
        return true;
    }
}