using CommunityToolkit.Maui.Converters;

namespace Principles.Converters
{
    public class ProgressMarkVariatyToBoolConverter : BaseConverterOneWay<UserHabit, bool>
    {

        public override bool DefaultConvertReturnValue { get; set; } = false;

        public override bool ConvertFrom( UserHabit value, CultureInfo? culture )
        {
            if(value is not null)
            {
                if(value.Type != TypeOfHabit.Mind && value.ProgressMarkVariaty == ProgressMarkVariaty.Numeric)
                {
                    return true;
                }
                return false;
            }
            return false;
        }
    }
}
