using CommunityToolkit.Mvvm.ComponentModel;

namespace Principles.Core.Models;

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
    private bool m_isArchived;

    [ObservableProperty]
    private string? m_description;
    [ObservableProperty]
    private UserGoal? m_goal;

    [ObservableProperty]
    private ObservableCollectionEx<UserHabitReminder>? m_reminders;
    
    public bool IsNew()
    {
        return Id == 0;
    }

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

    [ObservableProperty]
    private ObservableCollectionEx<ProgressOfHabit>? m_progresses;

    [ObservableProperty]
    private ListOfProgressOfHabit m_computedProgresses;

    [ObservableProperty]
    private ScoreList? m_scoreList;
    
    private bool m_isScoreListComputed = false;

    [ObservableProperty]
    private FrequencyOfHabit? m_frequency;

    [ObservableProperty]
    private string? m_colorName;
    
    [ObservableProperty]
    private int m_complexity;

    public UserHabit()
    {
        ScoreList = null; // Не створюємо ScoreList заздалегідь
        m_computedProgresses = new ListOfProgressOfHabit( this );
        m_isScoreListComputed = false; // Позначаємо що ScoreList ще не обчислений
    }
    
    /// <summary>
    /// Ленива властивість для ScoreList, яка обчислюється тільки коли потрібна в HabitDetailView
    /// </summary>
    public ScoreList LazyScoreList
    {
        get
        {
            if (!m_isScoreListComputed)
            {
                ScoreList = new ScoreList();
                m_isScoreListComputed = true;
            }
            return ScoreList!;
        }
    }
    
    /// <summary>
    /// Скидає ленивий ScoreList, щоб він був переобчислений при наступному зверненні
    /// </summary>
    public void ResetLazyScoreList()
    {
        ScoreList = null;
        m_isScoreListComputed = false;
    }

    public override string ToString()
    {
        return Name ?? "NULL";
    }

    public override bool Equals( object? obj )
    {
        return obj is UserHabit habit && habit.Id == Id;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public void MergeFrom(UserHabit userHabit)
    {
        Id = userHabit.Id;
        Name = userHabit.Name;
        Type = userHabit.Type;
        Status = userHabit.Status;
        if (userHabit.AreasOfLife is null)
        {
            AreasOfLife = null;
        }
        else
        {
            AreasOfLife = [];
            foreach (UserAreaOfLife areaOfLife in userHabit.AreasOfLife)
            {
                AreasOfLife.Add( areaOfLife );
            }
        }

        Priority = userHabit.Priority;
        IsArchived = userHabit.IsArchived;
        Description = userHabit.Description;
        Goal = userHabit.Goal?.Clone() as UserGoal;

        if (userHabit.Reminders is null)
        {
            Reminders = null;
        }
        else
        {
            Reminders = [];
            foreach (UserHabitReminder reminder in userHabit.Reminders)
            {
                Reminders.Add( (reminder.Clone() as UserHabitReminder)! );
            }
        }

        PercentageAchieved = userHabit.PercentageAchieved;
        Progresses = userHabit.Progresses;
        ComputedProgresses = userHabit.ComputedProgresses;
        // Не копіюємо ScoreList, він буде створений лениво при потребі
        Frequency = userHabit.Frequency?.Clone() as FrequencyOfHabit;
        ColorName = userHabit.ColorName;
        Complexity = userHabit.Complexity;
    }

    public void NotifyPropertyChanged( string propertyName )
    {
        OnPropertyChanged( propertyName );
    }
}
