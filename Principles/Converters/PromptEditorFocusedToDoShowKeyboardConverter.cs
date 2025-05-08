using System.Globalization;
using CommunityToolkit.Maui.Converters;

namespace Principles.Converters;

public class PromptEditorFocusedToDoShowKeyboardConverter : BaseConverterOneWay<PromptEditorFocused, bool>
{
    public override bool DefaultConvertReturnValue { get; set; } = false;

    public override bool ConvertFrom(PromptEditorFocused? value, CultureInfo? culture)
    {
        HelperViewModel? viewModel = value is null ? null : value?.ViewModel as HelperViewModel;
        return viewModel is not null && !viewModel.IsBusy && value!.IsEditorFocused;
    }
}
