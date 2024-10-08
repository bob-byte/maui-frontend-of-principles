
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

    void TRG_UserName_Focused( object sender, FocusEventArgs e )
    {
        L_Prompt.Text = LocStrings.YourName;
        ME_PromptResult.Text = ViewModel.UserName.Value;
        ME_PromptResult.HeightRequest = -1;
        ME_PromptResult.MaxLineCount = 1;
        ME_PromptResult.IsEndIconVisible = false;
        TE_UserName.Unfocus();

        DXP_Prompt.IsOpen = true;
    }

    void TGR_MainSlogan_Focused( object sender, FocusEventArgs e )
    {
        L_Prompt.Text = LocStrings.YourMainSlogan;
        L_TipText.Text = LocStrings.MainSloganExplanation;
        ME_PromptResult.Text = ViewModel.MainSlogan;
        ME_PromptResult.HeightRequest = 140;
        ME_PromptResult.MaximumHeightRequest = 140;
        ME_PromptResult.MaxLineCount = 5;
        ME_PromptResult.IsEndIconVisible = true;
        ME_MainSlogan.Unfocus();

        DXP_Prompt.IsOpen = true;
    }

    void TGR_Mission_Focused( object sender, FocusEventArgs e )
    {
        L_Prompt.Text = LocStrings.YourMission;
        L_TipText.Text = LocStrings.MissionExplanation;
        ME_PromptResult.Text = ViewModel.Mission;
        ME_PromptResult.HeightRequest = 170;
        ME_PromptResult.MaximumHeightRequest = 300;
        ME_PromptResult.MaxLineCount = 7;
        ME_PromptResult.IsEndIconVisible = true;
        ME_Mision.Unfocus();

        DXP_Prompt.IsOpen = true;
    }

    async void SB_Save_Clicked( object sender, EventArgs e )
    {
        IAsyncRelayCommand command;
        if (L_Prompt.Text == LocStrings.YourName)
        {
            command = ViewModel.SaveUserNameCommand;
        }
        else if (L_Prompt.Text == LocStrings.YourMainSlogan)
        {
            command = ViewModel.SaveMainSloganCommand;
        }
        else
        {
            command = ViewModel.SaveMissionCommand;
        }

        if (command.CanExecute( ME_PromptResult.Text ))
        {
            await command.ExecuteAsync( ME_PromptResult.Text );
        }

        DXP_Prompt.IsOpen = false;
    }

    private void SB_Cancel_Clicked( object sender, EventArgs e )
    {
        DXP_Prompt.IsOpen = false;
    }

    private void EI_Tip_Clicked( object sender, EventArgs e )
    {
        DXP_Tip.IsOpen = true;
    }

    private void B_Ok_Clicked( object sender, EventArgs e )
    {
        DXP_Tip.IsOpen = false;
    }
}
