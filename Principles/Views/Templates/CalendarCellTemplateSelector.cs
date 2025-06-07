using CommunityToolkit.Maui.Behaviors;

using DevExpress.Maui.Editors;

using System;

using TapGestureRecognizer = Microsoft.Maui.Controls.TapGestureRecognizer;

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
            View template;
            try
            {
                ProgressOfHabit progressOfHabit = m_viewModel.Habit.ComputedProgresses.Get( date );
                template = progressOfHabit.Value switch
                {
                    ProgressValue.YES_MANUAL => new YesManualCellTemplate( calendarCellData ),
                    ProgressValue.YES_AUTO => new YesAutoCellTemplate( calendarCellData ),
                    _ => new NonYesCellTemplate( calendarCellData )
                };
            }
            catch
            {
                template = new NonYesCellTemplate( calendarCellData );
            }

            template.GestureRecognizers.Add( new TapGestureRecognizer()
            {
                Command = m_viewModel.DateTappedCommand,
                CommandParameter = calendarCellData.Date,
                NumberOfTapsRequired = 1
            } );
            
            return template;
        });
        return result;
    }
}
