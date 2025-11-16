using SkiaSharp.Extended.UI.Controls;

namespace Principles.Models;

public partial class AppFeature : ObservableObject
{
    [ObservableProperty]
    private string? m_title;

    [ObservableProperty]
    private string? m_description;

    [ObservableProperty]
    private SKLottieImageSource? m_animation;
}
