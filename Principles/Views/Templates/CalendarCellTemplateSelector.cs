using CommunityToolkit.Maui.Behaviors;

using DevExpress.Maui.Editors;

using System;

using TapGestureRecognizer = Microsoft.Maui.Controls.TapGestureRecognizer;

namespace Principles.Views.Templates;

public class CalendarCellTemplateSelector : DataTemplateSelector
{
    private HabitDetailViewModel m_viewModel => m_contentPage.BindingContext as HabitDetailViewModel;
   
    private readonly DataTemplate m_yesManualTemplate;
    private readonly DataTemplate m_yesAutoTemplate;
    private readonly DataTemplate m_nonYesTemplate;
    private readonly ContentPage m_contentPage;

    public CalendarCellTemplateSelector(ContentPage contentPage)
    {
        //m_viewModel = ServiceLocator.Current!.GetRequiredService<HabitDetailViewModel>();
        
        m_yesManualTemplate = new DataTemplate( () => CreateTemplate( ProgressValue.YES_MANUAL ) );
        m_yesAutoTemplate = new DataTemplate( () => CreateTemplate( ProgressValue.YES_AUTO ) );
        m_nonYesTemplate = new DataTemplate( () => CreateTemplate( ProgressValue.NO ) );
        m_contentPage = contentPage;
    }

    protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
    {
        var calendarCellData = (CalendarCellData)item;
        var date = DateOnly.FromDateTime(calendarCellData.Date);

        try
        {
            ProgressOfHabit progress = (m_contentPage.BindingContext as HabitDetailViewModel).Habit.ComputedProgresses.Get(date);

            return progress.Value switch
            {
                ProgressValue.YES_MANUAL => m_yesManualTemplate,
                ProgressValue.YES_AUTO => m_yesAutoTemplate,
                _ => m_nonYesTemplate
            };
        }
        catch
        {
            return m_nonYesTemplate;
        }
    }

    private View CreateTemplate(int progressValue)
    {
        // Ideally this should not be parameterized like this if used across many cells,
        // but for DevExpress caching, this is safer than creating new templates on the fly.
        View template = progressValue switch
        {
            ProgressValue.YES_MANUAL => new YesManualCellTemplate(),
            ProgressValue.YES_AUTO => new YesAutoCellTemplate(),
            _ => new NonYesCellTemplate()
        };
        
        TapGestureRecognizer tap = new()
        {
            NumberOfTapsRequired = 1
        };
        tap.SetBinding(TapGestureRecognizer.CommandProperty, new Binding("DateTappedCommand", source: m_viewModel));
        tap.SetBinding(TapGestureRecognizer.CommandParameterProperty, "Date");
        template.GestureRecognizers.Add( tap );

        TouchBehavior longPress = new();
        longPress.Bind( TouchBehavior.LongPressCommandParameterProperty,
            path: $"Date" );
        longPress.Bind( TouchBehavior.LongPressCommandProperty, path: "ShowPopupOfProgressOfHabitCommand",
            source: m_contentPage.BindingContext );

        template.Behaviors.Add( longPress );

        return template;
    }
}
