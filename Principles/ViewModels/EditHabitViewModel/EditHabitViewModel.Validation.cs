using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Principles.ViewModels;
public partial class EditHabitViewModel
{
    [RelayCommand]
    private void ValidateHabitName()
    {
        NameOfHabit?.Validate();
    }

    private void InitValidations()
    {
        string isRequired = LocStrings.isRequired;

        if (NameOfHabit is null)
        {
            NameOfHabit = new ValidatableObject<string>();

            NameOfHabit.PropertyChanging += NameOfHabitOnPropertyChanging;
            NameOfHabit.PropertyChanged += NameOfHabitOnPropertyChanged;

            IValidationRule<string> rule = new IsNotNullOrWhiteSpaceRule( $"{LocStrings.TabData}. {LocStrings.FieldName} {isRequired}." );
            NameOfHabit.Validations.Add( rule );
        }
        else
        {
            NameOfHabit.SetIsValid();
        }

        NameOfHabit.Value = Habit!.Name ?? string.Empty;
    }
}
