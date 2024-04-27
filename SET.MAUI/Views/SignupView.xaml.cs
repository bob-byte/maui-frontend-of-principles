namespace SET.MAUI.Views;

public partial class SignupView : ContentPageBase
{
    public SignupView( SignupViewModel viewModel )
    {
        BindingContext = viewModel;
        ViewModel = viewModel;
        InitializeComponent();
        
        CCG_Genders.SelectChipAtIndex( index: 0 );
    }

    public SignupViewModel ViewModel { get; }

    private void CCG_Genders_SelectionChanged( object sender, EventArgs e )
    {
        if (CCG_Genders.SelectedChip != null)
        {
            Gender gender = (Gender)CCG_Genders.SelectedIndex;

            ViewModel.SetGenderCommand.Execute( gender.ToString() );
        }
    }
}
