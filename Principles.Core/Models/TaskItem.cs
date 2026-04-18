using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace Principles.Core.Models;

public partial class TaskItem : ObservableObject
{
    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private string? notes;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StateColor))] 
    private DateOnly? date;

    [ObservableProperty]
    private TimeOnly? time;

    public long Id { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StateColor))] 
    private bool isCompleted;

    public string StateColor 
    {
        get 
        {
            if (!IsCompleted && Date.HasValue && Date.Value < DateOnly.FromDateTime(DateTime.Today))
            {
                return "#F44336";
            }
            
            return "#2196F3"; 
        }
    }
}