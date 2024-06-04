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

            ContentView contentView = new();
            contentView.Bind(
                targetProperty: ContentView.ContentProperty,
                path: $"Item.Progresses[{progressIndex}]",
                converter: new ProgressToImgConverter()
            );

#if ANDROID
            TouchBehavior touchBehavior = new()
            {
                LongPressCommand = m_progressOfHabitsViewModel.SelectRowCommand,
                Command = m_progressOfHabitsViewModel.ChangeValueOfProgressOfHabitCommand
            };
            touchBehavior.Bind( TouchBehavior.LongPressCommandParameterProperty, "Item" );
            touchBehavior.Bind( TouchBehavior.CommandParameterProperty, $"Item.Progresses[{progressIndex}]" );
            contentView.Behaviors.Add( touchBehavior );
#else
            contentView.BindTapGesture(
                commandPath: "ChangeValueOfProgressOfHabitCommand",
                commandSource: m_progressOfHabitsViewModel,
                parameterPath: $"Item.Progresses[{progressIndex}]",
                numberOfTapsRequired: 1
            );
#endif

            return contentView;
        } );
    }

    protected override DataTemplate OnSelectTemplate( object item, BindableObject container )
    {
        return m_dataTemplate;
    }
}

