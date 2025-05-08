using CommunityToolkit.Maui.Behaviors;

using DevExpress.Maui.Editors;

using System;
namespace Principles.Views.Templates;

public class CalendarCellTemplateSelector : DataTemplateSelector
{
    private readonly HabitDetailViewModel m_viewModel = ServiceLocator.Current!.GetRequiredService<HabitDetailViewModel>();

    protected override DataTemplate OnSelectTemplate( object item, BindableObject container )
    {
        var calendarCellData = (CalendarCellData)item;
        var date = DateOnly.FromDateTime( calendarCellData.Date );

        DataTemplate result = new (() =>
        {
            ProgressOfHabit progressOfHabit = m_viewModel.Habit.ComputedProgresses.Get( date );

            View template = progressOfHabit.Value switch
            {
                ProgressValue.YES_MANUAL => new YesManualCellTemplate( calendarCellData ),
                ProgressValue.YES_AUTO => new YesAutoCellTemplate( calendarCellData ),
                _ => new NonYesCellTemplate( calendarCellData )
            };
            template.GestureRecognizers.Add( new TapGestureRecognizer()
            {
                Command = m_viewModel.DateTappedCommand,
                CommandParameter = calendarCellData.Date
            } );
            
            return template;
        });
        return result;
    }
}
