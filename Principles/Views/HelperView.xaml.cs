
using DevExpress.Maui.CollectionView;
using DevExpress.Maui.Core;

using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;

using System.Collections.Specialized;
namespace Principles.Views;

public partial class HelperView : ContentPageBase
{
    private readonly ILockDeviceOrientation m_deviceOrientationService;
    
    public HelperView(HelperViewModel viewModel)
	{
		BindingContext = viewModel;
		ViewModel = viewModel;

        m_deviceOrientationService = DependencyService.Get<ILockDeviceOrientation>();

        InitializeComponent();
        
#if IOS
        View originalContent = Content;
        
        SafeKeyboardAreaView wrappedContent = new()
        {
            Content = originalContent
        };
        Content = wrappedContent;
#endif

        ViewModel.DisplayMessages.CollectionChanged += DisplayMessages_CollectionChanged;
        ViewModel.ReferenceMessenger.Register<CopiedHelperResponse>( this, async ( sender, msg ) =>
        {
            DXP_Copy.IsOpen = true;
            await Task.Delay( 1500 );
            DXP_Copy.IsOpen = false;
        } );
    }
    
    protected override void OnAppearing()
    {
        base.OnAppearing();
#if ANDROID
        m_deviceOrientationService.LockOrientation( DeviceOrientation.Portrait );
        Microsoft.Maui.Controls.Application.Current.On<Microsoft.Maui.Controls.PlatformConfiguration.Android>()
            .UseWindowSoftInputModeAdjust( WindowSoftInputModeAdjust.Resize );
#endif
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
#if ANDROID
        m_deviceOrientationService.UnlockOrientation();
        Microsoft.Maui.Controls.Application.Current.On<Microsoft.Maui.Controls.PlatformConfiguration.Android>()
           .UseWindowSoftInputModeAdjust( WindowSoftInputModeAdjust.Pan );
#endif
    }

    private HelperViewModel ViewModel { get; }

    private void DisplayMessages_CollectionChanged( object? sender, NotifyCollectionChangedEventArgs e )
    {
		if(e.Action == NotifyCollectionChangedAction.Add)
		{
            DXC_DisplayMessages.ScrollTo( ViewModel.DisplayMessages.Count - 1 );

            if (ViewModel.DisplayMessages.Count > 3)
            {
                Thread.Sleep( 100 );
            }
        }
    }

    async void SB_AskQuestion_Clicked( System.Object sender, System.EventArgs e )
    {
        KeyboardHelper.HideKeyboard();
        E_Prompt.Unfocus();
        
#if ANDROID
        Shell.SetTabBarIsVisible( this, true );
#endif

        await AskQuestionAsync();
    }

    private async Task AskQuestionAsync()
	{
        if (ViewModel.AskUserQuestionCommand.CanExecute( null ))
        {
            await ViewModel.AskUserQuestionCommand.ExecuteAsync( null );
        }
    }

    void CPB_Helper_SizeChanged( System.Object sender, System.EventArgs e )
    {
        if(CPB_Helper.Width > 0)
        {
            MessageDataTemplateSelector.PageWidth = CPB_Helper.Width;
        }
    }

    void G_Title_SizeChanged( object sender, EventArgs e )
    {
        double titleWidth = G_Title.Width;
        double helpIconWidth = DXI_Help.Width;
        double infoIconWidth = DXI_Info.Width;
        if (titleWidth > 0 && helpIconWidth > 0 && infoIconWidth > 0)
        {
            double titleLabelWidth = titleWidth - helpIconWidth - infoIconWidth - 5;
            L_TitleText.WidthRequest = titleLabelWidth;
        }
    }

    private void E_Prompt_Focused( object sender, FocusEventArgs e )
    {
#if ANDROID
        Shell.SetTabBarIsVisible( this, false );
#endif
        
        ViewModel.PromptEditorFocused = new PromptEditorFocused(ViewModel, isEditorFocused: true);
    }

    private void E_Prompt_Unfocused( object sender, FocusEventArgs e )
    {
#if ANDROID
        Shell.SetTabBarIsVisible( this, true );
#endif
        
        ViewModel.PromptEditorFocused = new PromptEditorFocused(ViewModel, isEditorFocused: false);
    }

    private void TGR_HideKeyboard_Tapped( object? sender, TappedEventArgs e )
    {
        KeyboardHelper.HideKeyboard();
        
#if ANDROID
        Shell.SetTabBarIsVisible( this, true );
#endif
    }
}