using CommunityToolkit.Mvvm.ComponentModel;

namespace Principles.Core.Models;

public partial class UserHabit : ObservableObject, ICloneable, IEntity
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
        return LocalId == 0;
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
    private ScoreList m_scoreList;

    [ObservableProperty]
    private FrequencyOfHabit? m_frequency;

    [ObservableProperty]
    private string? m_colorName;
    
    [ObservableProperty]
    private int m_complexity;

    public long LocalId { get; set; }
    
    public DateTime LastModified { get; set; }

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
