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
    private string? m_name;

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

    public UserHabit()
    {
        m_scoreList = new ScoreList();
        m_computedProgresses = new ListOfProgressOfHabit( this );
    }

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

    public object Clone()
    {
        var habit = MemberwiseClone() as UserHabit;
        habit!.Frequency = Frequency?.Clone() as FrequencyOfHabit;

        if (Reminders is not null)
        {
            habit.Reminders = new ObservableCollectionEx<UserHabitReminder>();

            foreach (UserHabitReminder reminder in Reminders)
            {
                habit.Reminders.Add((reminder.Clone() as UserHabitReminder)!);
            }
        }

        habit.Goal = habit.Goal?.Clone() as UserGoal;

        return habit;
    }

    public void NotifyPropertyChanged( string propertyName )
    {
        OnPropertyChanged( propertyName );
    }
}
