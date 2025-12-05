using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Principles.ViewModels;
public partial class EditHabitViewModel
{
    [RelayCommand( CanExecute = nameof( CanSave ) )]
    private async Task SaveAsync()
    {
        bool canSaveHabit = await CanSaveHabitAsync();

        if (canSaveHabit)
        {
            Habit.Name = NameOfHabit.Value;
            EditUserHabitDto dto = new()
            {
                AreasOfLife = Habit.AreasOfLife!.ToList(),//get copy, because it will be changed
                ColorName = Habit.ColorName,
                Description = Habit.Description,
                Complexity = Habit.Complexity,
                Frequency = Habit.Frequency,
                Id = Habit.Id,
                Name = Habit.Name,
                Priority = Habit.Priority,
                Goal = Habit.Goal,
                Status = Habit.Status,
                Type = Habit.Type,
                Reminders = Habit.Reminders,
                IsArchived = Habit.IsArchived,
                PrioritizedHabits = new List<UserHabitWithPriority>()
            };

            dto.AreasOfLife!.Remove( AllAreasOfLifeAsOneItem );

            List<UserHabit> copyOfHabits = new( ServiceOfHabit.StoredUserHabits! );

            foreach (UserHabit habit in copyOfHabits)
            {
                dto.PrioritizedHabits.Add( new UserHabitWithPriority { Id = habit.Id, Priority = habit.Priority } );
            }

            bool isHabitSaved = false;
            
            IDictionary<string, object> routeParams = new Dictionary<string, object>
            {
                { "ShowAd", false }
            };
            
            await UiBusyFor( async () =>
            {
                UserHabit savedHabit = Habit;

                SaveHabitResponse response = await ServiceOfHabit.UpdateHabitAsync( dto );
                await HandleHabitSaveAsync( response );

                if (copyOfHabits.Count == 1)
                {
                    ReferenceMessenger.Send( new HabitSavedMessage( savedHabit ) );

                    if (dto.IsArchived && !m_isInHabitDetails)
                    {
                        ReferenceMessenger.Send( new ArchiveHabitMessage( savedHabit, doShowAllArchivedHabits: true ) );
                    }

                    await Navigation.GoBackAsync( routeParams );
                }
                else
                {
                    await Navigation.GoBackAsync( routeParams );

                    ReferenceMessenger.Send( new HabitSavedMessage( savedHabit ) );

                    if (dto.IsArchived && !m_isInHabitDetails)
                    {
                        ReferenceMessenger.Send( new ArchiveHabitMessage( savedHabit, doShowAllArchivedHabits: true ) );
                    }
                }

                isHabitSaved = true;
            } );

            if (isHabitSaved)
            {
                await TipService.ShowToastAsync( LocStrings.TheHabitSuccessfullySaved );
            }
        }
    }

    public bool CanSave()
    {
        NameOfHabit.Validate();

        return NameOfHabit.IsValid && !IsBusy && !IsLoadingHabitInfo;
    }
    
    private async Task<bool> CanSaveHabitAsync()
    {
        bool result;
        
        if (IsNewHabit)
        {
            result = ServiceOfHabit.IsItRecommendedToCreateNewHabit( Habit );

            if (!result)
            {
                List<ActionData> availableActions =
                [
                    new(LocStrings.ArchiveHabit, () =>
                        {
                            Habit.IsArchived = true;
                            result = true;
                            return Task.CompletedTask;
                        }
                    ),

                    new(LocStrings.CreateItAnyway, () =>
                        {
                            result = true;
                            return Task.CompletedTask;
                        }
                    )
                ];
                
                var multipleActionPopup = new MultipleActionPopup( availableActions,
                    LocStrings.DescriptionOfAdviceNotToWorkOnNewHabit, LocStrings.TitleOfAdviceNotToWorkOnNewHabit );

                await Shell.Current.ShowPopupAsync( multipleActionPopup );
            }
        }
        else
        {
            result = true;
        }
        
        return result; 
    }

    private async Task HandleHabitSaveAsync( SaveHabitResponse response )
    {
        Habit.Id = response.Id;
        Habit.Frequency!.Id = response.FrequencyId;

        if (response.ReminderIds is not null && Habit.Reminders?.Any() == true)
        {
            await ReminderService.RequestAccessToSendNotificationsAsync();

            for (int numReminder = 0; numReminder < response.ReminderIds.Count; numReminder++)
            {
                SaveHabitResponse.Reminder dtoOfReminder = response.ReminderIds[numReminder];
                UserHabitReminder habitReminder = Habit.Reminders[numReminder];
                habitReminder.Id = dtoOfReminder.Id;

                if (InactiveDaysToDelete.Count > 0)
                {
                    RemoveInactiveNotifications( InactiveDaysToDelete );
                }

                for (int numWeekDay = 0; numWeekDay < dtoOfReminder.DaysOfWeek?.Count; numWeekDay++)
                {
                    SaveHabitResponse.WeekDay dtoOfWeekDay = dtoOfReminder.DaysOfWeek[numWeekDay];
                    WeekDay? weekDay = habitReminder.DaysOfWeek.First( d => d.Type == dtoOfWeekDay.Type );
                    weekDay.Id = dtoOfWeekDay.Id;
                    weekDay.UserNotificationRequestId = dtoOfWeekDay.NotificationRequestId;

                    await ReminderService.AddNotificationToDeviceAsync( IsNewHabit, habitReminder, weekDay );
                }
            }
        }
    }
}
