using DevExpress.Maui.Editors;

namespace SET.MAUI.Views;

public partial class ProfileView : ContentPageBase
{
    private ProfileViewModel ViewModel { get; }

    public ProfileView(ProfileViewModel viewModel)
	{
        ViewModel = viewModel;
        BindingContext = viewModel;

		InitializeComponent();
    }

    void TGR_Name_Tapped( object sender, TappedEventArgs e )
    {
        L_Prompt.Text = LocStrings.YourName;
        ME_PromptResult.Text = ViewModel.UserName.Value;
        ME_PromptResult.MaxCharacterCount = 100;
        ME_PromptResult.HeightRequest = -1;
        ME_PromptResult.MaxLineCount = 1;
        ME_PromptResult.MaxCharacterCountOverflowMode = OverflowMode.LimitInput;
        TE_UserName.Unfocus();

        DXP_Prompt.IsOpen = true;
    }

    void TGR_MainSlogan_Tapped( object sender, TappedEventArgs e )
    {
        L_Prompt.Text = LocStrings.YourMainSlogan;
        ME_PromptResult.Text = ViewModel.MainSlogan;
        ME_PromptResult.MaxCharacterCount = 255;
        ME_PromptResult.MaxCharacterCountOverflowMode = OverflowMode.LimitInput;
        ME_PromptResult.HeightRequest = 140;
        ME_PromptResult.MaximumHeightRequest = 140;
        ME_PromptResult.MaxLineCount = -1;
        ME_MainSlogan.Unfocus();

        DXP_Prompt.IsOpen = true;
    }

    void TGR_Mission_Tapped( System.Object sender, Microsoft.Maui.Controls.TappedEventArgs e )
    {
        L_Prompt.Text = LocStrings.YourMission;
        ME_PromptResult.Text = ViewModel.Mission;
        ME_PromptResult.HeightRequest = 285;
        ME_PromptResult.MaximumHeightRequest = 300;
        ME_PromptResult.MaxLineCount = -1;
        ME_PromptResult.MaxCharacterCount = 4000;
        ME_PromptResult.MaxCharacterCountOverflowMode = OverflowMode.None;
        ME_Mision.Unfocus();

        DXP_Prompt.IsOpen = true;
    }

    async void SB_Save_Clicked( System.Object sender, System.EventArgs e )
    {
        DXP_Prompt.IsOpen = false;

        if (L_Prompt.Text == LocStrings.YourName)
        {
            ViewModel.UserName.Value = ME_PromptResult.Text;
            ViewModel.UserName.Validate();
            if (ViewModel.SaveUserNameCommand.CanExecute( null ))
            {
                await ViewModel.SaveUserNameCommand.ExecuteAsync( null );
            }
        }
        else if (L_Prompt.Text == LocStrings.YourMainSlogan)
        {
            ViewModel.MainSlogan = ME_PromptResult.Text;
            if (ViewModel.SaveMainSloganCommand.CanExecute( null ))
            {
                await ViewModel.SaveMainSloganCommand.ExecuteAsync( null );
            }
        }
        else if (L_Prompt.Text == LocStrings.YourMission)
        {
            ViewModel.Mission = ME_PromptResult.Text;
            if (ViewModel.SaveMissionCommand.CanExecute( null ))
            {
                await ViewModel.SaveMissionCommand.ExecuteAsync( null );
            }
        }
    }

    void ME_PromptResult_SizeChanged( System.Object sender, System.EventArgs e )
    {
        
    }
}