﻿using Foundation;

using System.Text.Json;

using Principles.Models;

// ReSharper disable once CheckNamespace
namespace Principles;

/// <inheritdoc />
internal sealed class AppStoreInfoImplementation : IAppStoreInfo
{
    /// <inheritdoc />
    public AppStoreInformation? CachedInformation { get; set; }

    /// <inheritdoc />
    public async Task<AppStoreInformation> GetInformationAsync( CancellationToken cancellationToken = default )
    {
        if (CachedInformation is null)
        {
            string packageName = AppInfo.Current.PackageName;
            LookupResult lookupResult = await LookupAppStoreInfoAsync(
                packageName,
                cancellationToken: cancellationToken
            ).ConfigureAwait( false );

            CachedInformation = new AppStoreInformation
            {
                ReleaseNotes = lookupResult.ReleaseNotes,
                LatestVersion = Version.Parse( lookupResult.Version ),
                StoreType = GetAppStoreType(),
                ExternalStoreUri = new Uri( lookupResult.TrackViewUrl ),
#if __IOS__
                InternalStoreUri = new Uri( $"itms-apps://itunes.apple.com/app/id{packageName}" ),
#elif __TVOS__
			    InternalStoreUri = new Uri($"com.apple.TVAppStore://itunes.apple.com/app/id{AppStoreInfo.Options.PackageName}"),
#elif __MACOS__
			    InternalStoreUri = new Uri($"macappstore://itunes.apple.com/app/id{AppStoreInfo.Options.PackageName}?mt=12"),
#endif
#if __IOS__
                InternalReviewUri = new Uri( $"itms-apps://itunes.apple.com/app/id{packageName}?action=write-review" ),
#elif __TVOS__
			    InternalReviewUri = new Uri($"com.apple.TVAppStore://itunes.apple.com/app/id{AppStoreInfo.Options.PackageName}?action=write-review"),
#elif __MACOS__
			    InternalReviewUri = new Uri($"macappstore://itunes.apple.com/app/id{AppStoreInfo.Options.PackageName}?action=write-review"),
#endif
            };
        }

        return CachedInformation;
    }

    private static async Task<LookupResult> LookupAppStoreInfoAsync(
        string bundleIdentifier,
        CancellationToken cancellationToken = default )
    {
        HttpClient client = new();

        CultureInfo currentCulture = CultureInfo.CurrentCulture;
        string countryCode;
        if (currentCulture.TwoLetterISOLanguageName == "uk" || currentCulture.TwoLetterISOLanguageName == "ua")
        {
            countryCode = "ua"; // Ukraine's country code is "ua", not "uk"
        }
        else
        {
            countryCode = "us";
        }

        string langCode = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName; // e.g. "uk"
        if (langCode != "uk")
        {
            langCode = "en"; // fallback to English if not Ukrainian
        }

        HttpResponseMessage response = await client.GetAsync(
            new Uri( $"https://itunes.apple.com/{countryCode}/lookup?bundleId={bundleIdentifier}&country={countryCode}&lang={langCode}&cache={Guid.NewGuid()}" ),
            cancellationToken ).ConfigureAwait( false );

        string json = await response.Content.ReadAsStringAsync(
            cancellationToken ).ConfigureAwait( false );

        LookupResponse lookup = JsonSerializer.Deserialize(
            json,
            SourceGenerationContext.Default.LookupResponse ) ?? new LookupResponse();

        LookupResult result =
            lookup.Results.FirstOrDefault() ??
            throw new InvalidOperationException(
               $"No lookup result found for bundle identifier '{bundleIdentifier}' " +
               $"and country code '{countryCode}'." );

        return result;
    }

    /// <inheritdoc />
    public async Task<bool> OpenApplicationInStoreAsync( CancellationToken cancellationToken )
    {
        return await Launcher.Default.OpenAsync( CachedInformation.ExternalStoreUri ).ConfigureAwait( false );
    }

    /// <summary>
    /// Retrieves the receipt URL path to determine if the app was installed via the App Store or TestFlight.
    /// </summary>
    /// <returns>The receipt path, or null if unavailable.</returns>
    private static AppStoreType GetAppStoreType()
    {
        // TODO: Modern way — StoreKit 2 - Seems MAUI is not supported in .NET 9 still
        // https://developer.apple.com/documentation/foundation/bundle/appstorereceipturl

        // A missing receipt means the app was deployed directly (Xcode, Enterprise, sideload).
        NSUrl receiptUrl = NSBundle.MainBundle.AppStoreReceiptUrl;
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (receiptUrl is null)
        {
            return AppStoreType.ManualInstallation;
        }

        // App Store receipts end with “…/receipt”, TestFlight with “…/sandboxReceipt”.
        return receiptUrl.LastPathComponent?.Equals( "sandboxReceipt", StringComparison.OrdinalIgnoreCase ) == true
            ? AppStoreType.AppleTestFlight
            : AppStoreType.AppleAppStore;
    }
}