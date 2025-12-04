
using Serilog.Events;
using Serilog;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Maui.Handlers;
using DevExpress.Maui.Editors;
using Microsoft.Maui;
using SkiaSharp.Views.Maui.Controls.Hosting;
using DevExpress.Maui.Editors.Internal;

using System.Reflection;
using LiveChartsCore.SkiaSharpView.Maui;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;

using Plugin.AdMob;
using Plugin.AdMob.Configuration;

#if IOS
using Microsoft.Maui.Platform;
#endif
#if ANDROID
using AndroidX.AppCompat.Widget;
#endif

namespace Principles;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        MauiAppBuilder builder = MauiApp.CreateBuilder();
         
        SetupSerilog();

        builder
            .UseMauiApp<App>()
            .UseAdMob(
                androidDefaultInterstitialAdUnitId: "ca-app-pub-6307192789973793/7567835848",
                iosDefaultInterstitialAdUnitId: "ca-app-pub-6307192789973793/6132315308",
                automaticallyAskForConsent: false )
            .UseSkiaSharp()
            .UseLiveCharts()
            .UseDevExpressControls()
            .UseDevExpressEditors()
            .UseDevExpressCollectionView()
            .UseDevExpressDataGrid()
            .UseDevExpress( useLocalization: false ) //register handlers for all DevExpress controls
            .UseLocalNotification()
            .UseMauiCommunityToolkit()
            .UseMauiCommunityToolkitMarkup()
            .ConfigureEssentials( essentials =>
            {
                essentials.UseVersionTracking();
            } )
            .ConfigureFonts( fonts =>
            {
                fonts.AddFont( "FontAwesome6FreeBrands.otf", "FontAwesomeBrands" );
                fonts.AddFont( "FontAwesome6FreeRegular.otf", "FontAwesomeRegular" );
                fonts.AddFont( "FontAwesome6FreeSolid.otf", "FontAwesomeSolid" );
                fonts.AddFont( "Mona-Sans-Bold.ttf", "MonaSansBold" );
                fonts.AddFont( "Mona-Sans-Medium.ttf", "MonaSansMedium" );
                fonts.AddFont( "OpenSans-Regular.ttf", "OpenSansRegular" );
                fonts.AddFont( "OpenSans-Semibold.ttf", "OpenSansSemibold" );
                fonts.AddFont( "roboto-regular.ttf", "Roboto" );
                fonts.AddFont( "roboto-medium.ttf", "Roboto-Medium" );
                fonts.AddFont( "roboto-bold.ttf", "Roboto-Bold" );
                fonts.AddFont( "univia-pro-regular.ttf", "Univia-Pro" );
                fonts.AddFont( "univia-pro-medium.ttf", "Univia-Pro Medium" );
                fonts.AddFont( "CambriaBold.ttf", "CambriaBold" );
            } )
            .Services
            .RegisterAppCore()
            .RegisterMauiServices()
            .RegisterViewModels()
            .RegisterViews();
        
        var assembly = Assembly.GetExecutingAssembly();

        //TODO: replace appsettings.json and implementation of the config to Principles.Core project

#if LOCALDEBUG
        using Stream? stream = assembly.GetManifestResourceStream( $"{assembly.GetName().Name}.appsettings.Development.json" );
#else
        using Stream? stream = assembly.GetManifestResourceStream( $"{assembly.GetName().Name}.appsettings.json" );
#endif

        if (stream is not null)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddJsonStream( stream )
                .Build();

            builder.Configuration.AddConfiguration( configuration );
            builder.Services.AddSingleton<IConfiguration>( configuration );
        }

        AllowMultiLineTruncation();
        
#if IOS
        //hide Done button above keyboard for Editor control
        EditorHandler.Mapper.AppendToMapping("NoAccessoryView", (handler, view) =>
        {
            if (handler.PlatformView is UITextView uiTextView)
            {
                uiTextView.InputAccessoryView = null;
            }
        });
#endif

        return builder.Build();
    }

    public static IServiceCollection RegisterMauiServices( this IServiceCollection services )
    {
        services.AddSingleton<ICachingService, CachingService>();
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddSingleton<INavigationService, MauiNavigationService>();
        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<ILaunchUriHelper, LaunchUriHelper>();
        services.AddSingleton<IReminderService, ReminderService>();
        services.AddSingleton<ITipService, TipService>();
        services.AddSingleton<IAdService, AdService>();
        services.AddSingleton<IAppOpenTrackerService, AppOpenTrackerService>();

        return services;
    }

    public static IServiceCollection RegisterViewModels( this IServiceCollection services )
    {
        services.AddSingleton<LoginViewModel>();
        services.AddSingleton<SignupViewModel>();
        services.AddSingleton<SettingsViewModel>();
        services.AddSingleton<HelperViewModel>();
        services.AddSingleton<ProgressOfHabitsViewModel>();
        services.AddSingleton<EditHabitViewModel>();
        services.AddSingleton<ProfileViewModel>();
        services.AddSingleton<ForgetPasswordViewModel>();
        services.AddSingleton<StartupViewModel>();
        services.AddSingleton<MultipleActionPopupViewModel>();
        services.AddSingleton<ChangePasswordViewModel>();
        services.AddSingleton<ConfirmEmailPopupViewModel>();
        services.AddSingleton<HabitDetailViewModel>();
        services.AddSingleton<AppBenefitsViewModel>();

        return services;
    }

    public static IServiceCollection RegisterViews( this IServiceCollection services )
    {
        services.AddTransient<LoginView>();
        services.AddTransient<SignupView>();
        services.AddTransient<SettingsView>();
        services.AddTransient<ProgressOfHabitsView>();
        services.AddTransient<EditHabitView>();
        services.AddTransient<HelperView>();
        services.AddTransient<ProfileView>();
        services.AddTransient<ForgetPasswordView>();
        services.AddTransient<StartupView>();
        services.AddTransient<HabitDetailView>();
        services.AddTransient<ChangePasswordView>();
        services.AddTransient<AppBenefitsView>();

        return services;
    }

    private static void SetupSerilog()
    {
        Log.Logger = new LoggerConfiguration()
#if DEBUG
#if IOS
            .WriteTo.NSLog( restrictedToMinimumLevel: LogEventLevel.Information )
#else
            .WriteTo.AndroidLog( restrictedToMinimumLevel: LogEventLevel.Information )
#endif
#else
            //for Release mode and any OS
            .WriteTo.Sink( new LoggerToServer( CreateSaveLogRequest ), LogEventLevel.Information )
#endif
            .Enrich.FromLogContext()
            .CreateLogger();
    }

#if !DEBUG
    private static SaveLogRequest CreateSaveLogRequest( LogEvent logEvent, IServiceProvider serviceProvider )
    {
        IDeviceInfo deviceInfo = DeviceInfo.Current;
        IAppInfo appInfo = AppInfo.Current;
        
        string? stackTrace = null;
        if (logEvent.Exception is not null)
        {
            string exception = logEvent.Exception.ToString();
            int indexOfFirstNewLine = exception.IndexOf( Environment.NewLine, StringComparison.Ordinal );
            stackTrace = exception.Substring( indexOfFirstNewLine );
        }

        SaveLogRequest result = new(
            DeviceOs: $"{deviceInfo.Platform} {deviceInfo.VersionString}",
            DeviceModelName: $"{deviceInfo.Name} {deviceInfo.Model}",
            DeviceType: deviceInfo.DeviceType.ToString(),
            DeviceManufacturer: deviceInfo.Manufacturer,
            AppVersion: appInfo.VersionString,
            LogType: logEvent.Level.ToString(),
            LogMessage: logEvent.MessageTemplate.Text,
            StackTrace: stackTrace
        );
        return result;
    }
#endif

    private static void AllowMultiLineTruncation()
    {
        static void UpdateMaxLines( ILabelHandler handler, ILabel label )
        {
#if ANDROID
            AppCompatTextView textView = handler.PlatformView;

            if (label is Label controlsLabel
                && textView.Ellipsize == Android.Text.TextUtils.TruncateAt.End
                && controlsLabel.MaxLines != -1)
            {
                textView.SetMaxLines( controlsLabel.MaxLines );
            }
#elif IOS
            MauiLabel textView = handler.PlatformView;
            if( label is Label controlsLabel
                && textView.LineBreakMode == UILineBreakMode.TailTruncation
                && controlsLabel.MaxLines != -1 )
            {
              textView.Lines = controlsLabel.MaxLines;
            }  
#endif
        }
        
        LabelHandler.Mapper.AppendToMapping(
           nameof( Label.LineBreakMode ), UpdateMaxLines );

        LabelHandler.Mapper.AppendToMapping(
          nameof( Label.MaxLines ), UpdateMaxLines );
    }
}