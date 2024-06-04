
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
#if IOS
using Microsoft.Maui.Platform;
#endif
#if ANDROID
using AndroidX.AppCompat.Widget;
#endif

namespace SET.MAUI;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        MauiAppBuilder builder = MauiApp.CreateBuilder();

        SetupSerilog();

        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseMauiCommunityToolkitMarkup()
            .UseDevExpress( useLocalization: true ) //register handlers for all DevExpress controls
            .UseSkiaSharp()
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
            } )
            .Services
            .RegisterAppCore()
            .RegisterMauiServices()
            .RegisterViewModels()
            .RegisterViews();
        
        AllowMultiLineTruncation();

        return builder.Build();
    }

    public static IServiceCollection RegisterMauiServices( this IServiceCollection services )
    {
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddSingleton<INavigationService, MauiNavigationService>();
        services.AddSingleton<IDialogService, DialogService>();

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
        services.AddSingleton<UserAgreementViewModel>();
        services.AddSingleton<PrivacyPolicyViewModel>();

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
        services.AddTransient<UserAgreementView>();
        services.AddTransient<PrivacyPolicyView>();

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
            .WriteTo.Sink( new LoggerToServer( CreateSaveLogRequest ), LogEventLevel.Error )
#endif
            .Enrich.FromLogContext()
            .CreateLogger();
    }

    private static SaveLogRequest CreateSaveLogRequest( LogEvent logEvent, IServiceProvider serviceProvider )
    {
        IDeviceInfo deviceInfo = DeviceInfo.Current;
        IAppInfo appInfo = AppInfo.Current;

        ISettingsService settingsService = serviceProvider.GetRequiredService<ISettingsService>();

        _ = long.TryParse( settingsService.UserId, out long userId );

        SaveLogRequest result = new(
            UserId: userId,
            DeviceOs: $"{deviceInfo.Platform} {deviceInfo.VersionString}",
            DeviceModelName: $"{deviceInfo.Name} {deviceInfo.Model}",
            DeviceType: deviceInfo.DeviceType.ToString(),
            DeviceManufacturer: deviceInfo.Manufacturer,
            AppVersion: appInfo.VersionString,
            LogType: logEvent.Level.ToString(),
            LogMessage: logEvent.MessageTemplate.Text,
            StackTrace: logEvent.Exception?.StackTrace
        );
        return result;
    }

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