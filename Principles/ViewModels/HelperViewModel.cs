using Azure.AI.OpenAI;

using DevExpress.Data.Extensions;

using OpenAI;
using OpenAI.Chat;

namespace Principles.ViewModels;

public partial class HelperViewModel : BaseViewModel
{
    private readonly IAiChatService m_aiChatService;
    [ObservableProperty]
    private string? m_prompt;

    [ObservableProperty]
    private bool m_isAnimationVisible;

    [ObservableProperty]
    private ObservableCollectionEx<DisplayMessage> m_displayMessages;

    private CancellationTokenSource m_cancellationSource;

    public HelperViewModel( IServiceProvider serviceProvider, IAiChatService aiChatService )
        : base( serviceProvider )
    {
        IsAnimationVisible = true;
        m_displayMessages = new ObservableCollectionEx<DisplayMessage>();
        m_aiChatService = aiChatService;
        m_cancellationSource = new CancellationTokenSource();

        ReferenceMessenger.Register<UserLoggedOutMessage>( this, ( sender, msg ) =>
        {
            DefaultHandleLogout( msg );

            Prompt = string.Empty;
            DisplayMessages.Clear();
            m_aiChatService.ClearChat();
            IsAnimationVisible = true;
        } );
    }

    public override async Task InitializeAsync( object? parameter = null )
    {
        await base.InitializeAsync( parameter );
    }

    private void AddUserMessage(string msg)
    {
        if(DisplayMessages.Count == 0)
        {
            IsAnimationVisible = false;
        }

        DisplayMessages.Add( new DisplayMessage
        {
            IsUserMessage = true,
            Text = msg,
            ViewModel = this
        } );
    }

    [RelayCommand( CanExecute = nameof( CanAskQuestion ) )]
    private async Task AskUserQuestionAsync()
    {
        if (string.IsNullOrWhiteSpace( Prompt ))
        {
            return;
        }
        else
        {
            IsBusy = true;

            try
            {
                string promptCopy = Prompt;
                Prompt = string.Empty;

                AddUserMessage( promptCopy );

                DisplayMessage helperMsg = new()
                {
                    IsUserMessage = false,
                    Text = string.Empty,
                    ViewModel = this
                };
                DisplayMessages.Add( helperMsg );

                bool doTryAgain;

                do
                {
                    try
                    {
                        helperMsg.Text = string.Empty;
                        CancellationToken cancellationToken = m_cancellationSource.Token;

                        try
                        {
                            ChatCompletion chatResponse = await m_aiChatService.GetAnswerStreamAsync( promptCopy );

                            string fullResponse = chatResponse.Content[0].Text;

                            string[] words = fullResponse.Split( ' ' );

                            foreach (string word in words)
                            {
                                helperMsg.Text += word + " ";
                                await Task.Delay( millisecondsDelay: 65 );
                            }
                        }
                        catch (TaskCanceledException) { }
                        catch (OperationCanceledException) { }

                        if (cancellationToken.IsCancellationRequested)
                        {
                            ResetCancellationOfAnswerGeneration();
                        }

                        m_aiChatService.AddChatAnswer( helperMsg.Text );
                        doTryAgain = false;
                    }
                    catch (Exception ex)
                    {
                        doTryAgain = await DoRetryOperationOnErrorAsync( ex );
                        if (!doTryAgain)
                        {
                            if (string.IsNullOrWhiteSpace( helperMsg.Text ))
                            {
                                helperMsg.Text = LocStrings.ErrorOccurred;
                            }
                            else
                            {
                                helperMsg.Text += Environment.NewLine + Environment.NewLine + LocStrings.ErrorOccurred;
                            }
                        }
                    }
                }
                while (doTryAgain);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }

    private bool CanAskQuestion()
    {
        return !m_cancellationSource.IsCancellationRequested && !IsBusy;
    }

    [RelayCommand(CanExecute = nameof(CanCancel))]
    private void CancelAnswerGeneration()
    {
        m_cancellationSource.Cancel();
    }

    private bool CanCancel()
    {
        return IsBusy && !m_cancellationSource.IsCancellationRequested;
    }

    private void ResetCancellationOfAnswerGeneration()
    {
        m_cancellationSource = new CancellationTokenSource();
    }

    [RelayCommand]
    private Task ShowHelperInfoSnackbar(VisualElement visualElement)
    {
        return visualElement.DisplaySnackbar(
            LocStrings.HelperWarning,
            duration: TimeSpan.FromSeconds( 12 ),
            visualOptions: SnackbarHelper.DefaultOptions()
        );
    }
}