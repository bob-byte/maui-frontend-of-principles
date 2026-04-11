namespace Principles.Models;

public class AppStoreInformation
{
    /// <summary>
    /// The latest version.
    /// </summary>
    public Version LatestVersion { get; init; } = new(major: 0, minor: 0, build: 0, revision: 0);
    
    /// <summary>
    /// The external store URL(Accessible from the Internet).
    /// </summary>
    public Uri ExternalStoreUri { get; init; } = new("about:blank");
    
    /// <summary>
    /// The internal store URL(Available for opening within a specific platform).
    /// </summary>
    public Uri InternalStoreUri { get; init; } = new("about:blank");
    
    /// <summary>
    /// The internal store review URL(Available for opening within a specific platform).
    /// </summary>
    public Uri InternalReviewUri { get; init; } = new("about:blank");
    
    /// <summary>
    /// The release notes in App Store.
    /// </summary>
    public string ReleaseNotes { get; init; } = string.Empty;

    /// <summary>
    /// Store type
    /// </summary>
    public AppStoreType StoreType { get; init; }
}