using Azure.AI.OpenAI;

namespace SET.MAUI.ViewModels;

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
        Title = LocStrings.ChatWithHelper;
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
        await InitUserInfoAsync();
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
            await UiBusyFor( async () =>
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
                CancellationToken cancellationToken = m_cancellationSource.Token;

                try
                {
                    await foreach (StreamingChatCompletionsUpdate chatUpdate in await m_aiChatService.GetAnswerStreamAsync( promptCopy, choiceCount: 1, cancellationToken ))
                    {
                        if (!string.IsNullOrWhiteSpace( chatUpdate.ContentUpdate ))
                        {
                            helperMsg.Text += chatUpdate.ContentUpdate.Replace("`", "");
                            await Task.Delay( millisecondsDelay: 65 );
                        }
                    }
                }
                catch (TaskCanceledException) { }
                catch (OperationCanceledException) { }

                if (cancellationToken.IsCancellationRequested)
                {
                    ResetCancellationOfAnswerGeneration();
                }

                m_aiChatService.AddChatAnswer( helperMsg.Text );
            } );
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
}