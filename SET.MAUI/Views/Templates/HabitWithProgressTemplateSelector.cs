using CommunityToolkit.Maui.Behaviors;

using DevExpress.Maui.DataGrid;
using DevExpress.Maui.Editors;

using System;
using System.Collections.Concurrent;

namespace SET.MAUI.Views.Templates;

public class HabitWithProgressTemplateSelector : DataTemplateSelector
{
    private readonly ProgressOfHabitsViewModel m_progressOfHabitsViewModel;
    private readonly DataTemplate m_dataTemplate;

    public HabitWithProgressTemplateSelector(ProgressOfHabitsViewModel viewModel, DateOnly date)
    {
        m_progressOfHabitsViewModel = viewModel;
        m_dataTemplate = new DataTemplate( loadTemplate: () =>
        {
            CheckEdit checkEdit = new()
            {
                Margin = new Thickness( 15, 0, 0, 0 )
            };

            IValueConverter progressValueConverter = new ProgressOfHabitValueConverter();
            int progressIndex = 0;
            ProgressOfHabit[] progresses = m_progressOfHabitsViewModel.UserHabits[0].Progresses!.ToArray();
            for (int numProgress = 0; numProgress < progresses.Length; numProgress++)
            {
                if (progresses[numProgress].Date == date)
                {
                    progressIndex = numProgress;
                    break;
                }
            }

            checkEdit.Bind(
                CheckEdit.IsCheckedProperty,
                path: $"Item.Progresses[{progressIndex}].Value",
                converter: progressValueConverter,
                mode: BindingMode.OneWay
            );

            checkEdit.CheckedCheckBoxImage = ImageSource.FromFile( "fire_second" );

            var primaryColor = (Color)Application.Current!.Resources["Primary"];
            checkEdit.CheckedCheckBoxColor = primaryColor;

            IValueConverter uncheckedProgressToImgConverter = new UncheckedProgressToImgConverter();
            checkEdit.Bind(
                targetProperty: CheckEdit.UncheckedCheckBoxImageProperty,
                path: $"Item.Progresses[{progressIndex}]",
                converter: uncheckedProgressToImgConverter
            );

#if IOS
            checkEdit.BindTapGesture(
                commandPath: "ChangeValueOfProgressOfHabitCommand",
                commandSource: m_progressOfHabitsViewModel,
                parameterPath: $"Item.Progresses[{progressIndex}]",
                numberOfTapsRequired: 1
            );
#else
            EventToCommandBehavior eventToCommandBehavior = new()
            {
                EventName = "CheckedChanged"
            };
            eventToCommandBehavior.SetBinding(
                EventToCommandBehavior.CommandProperty,
                new Binding( path: "ChangeValueOfProgressOfHabitCommand", source: m_progressOfHabitsViewModel )
            );
            eventToCommandBehavior.SetBinding(
                EventToCommandBehavior.CommandParameterProperty,
                new Binding( path: $"Item.Progresses[{progressIndex}]")
            );

            checkEdit.Behaviors.Add( eventToCommandBehavior );
#endif

            return checkEdit;
        } );
    }

    protected override DataTemplate OnSelectTemplate( object item, BindableObject container )
    {
        return m_dataTemplate;
    }
}

