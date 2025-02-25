using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Principles.ViewModels;
public partial class EditHabitViewModel
{
    private async void AreasOfLife_CollectionChanged( object? sender, NotifyCollectionChangedEventArgs e )
    {
        if (!IsLoadingHabitInfo && e.Action == NotifyCollectionChangedAction.Add && e.NewItems is not null)
        {
            List<UserAreaOfLife> areasOfLife = Habit.AreasOfLife!.ToList();

            bool isAddedAllAreasOfLifeAsOneItem = e.NewItems.Contains( AllAreasOfLifeAsOneItem );
            if (isAddedAllAreasOfLifeAsOneItem)
            {
                for (int index = areasOfLife.Count - 1; index >= 0; index--)
                {
                    if (areasOfLife[index] != AllAreasOfLifeAsOneItem)
                    {
                        areasOfLife.RemoveAt( index );
                    }
                }
            }
            else
            {
                areasOfLife.Remove( AllAreasOfLifeAsOneItem );
            }

            if (areasOfLife.Count != Habit.AreasOfLife!.Count)
            {
                //to change collection after collection change event
                await Task.Delay( millisecondsDelay: 5 );
                Habit.AreasOfLife!.Reload( areasOfLife );
            }
        }
    }

    private async Task ReloadAllAreasOfLifeAsync()
    {
        try
        {
            List<UserAreaOfLife> areasOfLife = await AreaOfLifeService.UserAreasOfLife();
            foreach (UserAreaOfLife area in areasOfLife)
            {
                //localize names
                string? locName = LocManager[area.Name!];
                if (!string.IsNullOrWhiteSpace( locName ))
                {
                    area.Name = locName;
                }
            }
            areasOfLife.Insert( index: 0, AllAreasOfLifeAsOneItem );

            AllUserAreasOfLife = new ObservableCollectionEx<UserAreaOfLife>( areasOfLife );
        }
        catch
        {
            AllUserAreasOfLife.Clear();
        }
    }
}
