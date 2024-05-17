using CommunityToolkit.Mvvm.ComponentModel;

namespace SET.Core.Models;

public partial class UserHabit : ObservableObject
{
    [ObservableProperty]
    private long m_id;
    [ObservableProperty]
    private string? m_name;

    [ObservableProperty]
    private TypeOfHabit m_type;

    [ObservableProperty]
    private StatusOfHabit m_status;

    [ObservableProperty]
    private ObservableCollectionEx<UserAreaOfLife>? m_areasOfLife;

    [ObservableProperty]
    private int m_priority;

    [ObservableProperty]
    private string? m_description;
    [ObservableProperty]
    private string? m_reasonToFollow;
    [ObservableProperty]
    private string? m_question;
    [ObservableProperty]
    private bool m_isShownInProgressTab;

    private double m_percentageAchieved;
    public double PercentageAchieved
    {
        get => m_percentageAchieved;
        set
        {
            OnPropertyChanging();
            m_percentageAchieved = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Will be used for statistics of habit
    /// </summary>
    [ObservableProperty]
    private int m_followedCount;
    
    public double? PreviousPercentageAchieved { get; set; }
    public int? CountOfFollowedPerSpecificInterval { get; set; }

    [ObservableProperty]
    private ObservableCollectionEx<ProgressOfHabit>? m_progresses;

    [ObservableProperty]
    private ListOfProgressOfHabit m_computedProgresses;

    [ObservableProperty]
    private ScoreList m_scoreList;

    [ObservableProperty]
    private ObservableCollectionEx<UserHabit>? m_parentHabits;
    [ObservableProperty]
    private ObservableCollectionEx<UserHabit>? m_subHabits;

    [ObservableProperty]
    private FrequencyOfHabit? m_frequency;

    [ObservableProperty]
    private string? m_colorName;

    [ObservableProperty]
    private bool m_hasSubhabits;

    [ObservableProperty]
    private bool m_canHasSubhabits;

    [ObservableProperty]
    private Reminder? m_reminder;
    [ObservableProperty]
    private int m_complexity;

    public UserHabit()
    {
        m_scoreList = new ScoreList();
        m_computedProgresses = new ListOfProgressOfHabit( this );
    }

    public long UserId { get; set; }

    public override string ToString()
    {
        return Name ?? "NULL";
    }
}
