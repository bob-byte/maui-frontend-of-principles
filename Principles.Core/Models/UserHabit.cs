using CommunityToolkit.Mvvm.ComponentModel;

namespace Principles.Core.Models;

[Table( "UserHabit" )]
public partial class UserHabit : ObservableObject, ICloneable, IEntity
{
    [ObservableProperty]
    private long m_id;

    [PrimaryKey, AutoIncrement]
    public long LocalId { get; set; }

    public DateTime LastModified { get; set; }

    [ObservableProperty]
    private string m_name;

    [ObservableProperty]
    private TypeOfHabit m_type;

    [ObservableProperty]
    private StatusOfHabit m_status;

    [ObservableProperty]
    [property: Ignore]
    private ObservableCollectionEx<UserAreaOfLife>? m_areasOfLife;

    [ObservableProperty]
    private int m_priority;

    [ObservableProperty]
    private bool m_isArchived;

    [ObservableProperty]
    private string? m_description;
    [ObservableProperty]
    [property: Ignore]
    private UserGoal? m_goal;

    public long? GoalLocalId { get; set; }

    [ObservableProperty]
    [property: Ignore]
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
    public DateTime? ArchivingTime { get; set; }

    public bool IsNew()
    {
        return LocalId == 0;
    }

    private double m_percentageAchieved;
    [Ignore]
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
    [property: Ignore]
    private ObservableCollectionEx<ProgressOfHabit>? m_progresses;

    [ObservableProperty]
    [property: Ignore]
    private ListOfProgressOfHabit m_computedProgresses;

    [ObservableProperty]
    [property: Ignore]
    private ScoreList m_scoreList;

    [ObservableProperty]
    [property: Ignore]
    private FrequencyOfHabit? m_frequency;

    public long FrequencyLocalId { get; set; }

    [ObservableProperty]
    private string? m_colorName;
    [ObservableProperty]
    private int m_complexity;

    [ObservableProperty]
    private int m_defaultProgressValue;

    public UserHabit()
    {
        m_scoreList = new ScoreList();
        m_computedProgresses = new ListOfProgressOfHabit( this );
    }

    public bool IsNumerical => ProgressMarkVariaty == ProgressMarkVariaty.Numeric;
    
    public override string ToString()
    {
        return Name ?? string.Empty;
    }

    public override bool Equals( object? obj )
    {
        return obj is UserHabit habit &&
               ((Id != 0 && habit.Id == Id) || (Id == 0 && LocalId != 0 && habit.LocalId == LocalId));
    }

    public override int GetHashCode()
    {
        return Id != 0 ? Id.GetHashCode() : LocalId.GetHashCode();
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
        ArchivingTime = userHabit.ArchivingTime;
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

    public object Clone()
    {
        UserHabit habit = (UserHabit)MemberwiseClone();
        if (habit.AreasOfLife is not null)
        {
            habit.AreasOfLife = new ObservableCollectionEx<UserAreaOfLife>( habit.AreasOfLife );
        }

        if (habit.Reminders is not null)
        {
            habit.Reminders = new ObservableCollectionEx<UserHabitReminder>(
                habit.Reminders.Select( reminder => (reminder.Clone() as UserHabitReminder)! )
            );
        }

        if (habit.Progresses is not null)
        {
            habit.Progresses = new ObservableCollectionEx<ProgressOfHabit>( habit.Progresses );
        }

        habit.Goal = habit.Goal?.Clone() as UserGoal;
        habit.Frequency = habit.Frequency?.Clone() as FrequencyOfHabit;
        return habit;
    }

    public void NotifyPropertyChanged( string propertyName )
    {
        OnPropertyChanged( propertyName );
    }
}
