using CommunityToolkit.Mvvm.ComponentModel;

namespace Principles.Core.Models;

public partial class UserHabit : ObservableObject
{
    [ObservableProperty]
    private long m_id;
    [ObservableProperty]
    private string m_name;

    [ObservableProperty]
    private TypeOfHabit m_type;

    [ObservableProperty]
    private StatusOfHabit m_status;

    [ObservableProperty]
    private ObservableCollectionEx<UserAreaOfLife> m_areasOfLife;

    [ObservableProperty]
    private int m_priority;

    [ObservableProperty]
    private bool m_isArchived;

    [ObservableProperty]
    private string? m_description;
    [ObservableProperty]
    private UserGoal m_goal;

    [ObservableProperty]
    private ObservableCollectionEx<UserHabitReminder>? m_reminders;

    [ObservableProperty]
    private string? m_unit;

    [ObservableProperty]
    private double? m_targetPerOneTime;

    [ObservableProperty]
    private NumericalHabitType m_targetType;

    [ObservableProperty]
    private double? m_minRate;

    [ObservableProperty]
    private double? m_maxRate;
    [ObservableProperty]
    private ProgressMarkVariaty m_progressMarkVariaty;

    public bool IsNew()
    {
        return Id == 0;
    }

    public double PercentageAchieved
    {
        get => field;
        set
        {
            OnPropertyChanging();
            field = value;
            OnPropertyChanged();
        }
    }

    [ObservableProperty]
    private ObservableCollectionEx<ProgressOfHabit>? m_progresses;

    [ObservableProperty]
    private ListOfProgressOfHabit m_computedProgresses;

    [ObservableProperty]
    private ScoreList m_scoreList;

    [ObservableProperty]
    private FrequencyOfHabit m_frequency;

    [ObservableProperty]
    private string m_colorName;
    
    [ObservableProperty]
    private int m_complexity;

    [ObservableProperty]
    private int m_defaultProgressValue;

    public UserHabit()
    {
        m_scoreList = new ScoreList();
        m_computedProgresses = new ListOfProgressOfHabit( this );
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
        ScoreList = userHabit.ScoreList;
        Frequency = userHabit.Frequency?.Clone() as FrequencyOfHabit;
        ColorName = userHabit.ColorName;
        Complexity = userHabit.Complexity;
    }

    public void NotifyPropertyChanged( string propertyName )
    {
        OnPropertyChanged( propertyName );
    }
}
