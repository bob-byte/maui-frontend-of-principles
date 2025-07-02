namespace Principles.Core.Models;

public static class HabitComplexity
{
    public static int ToDaysCount( double complexity )
    {
        int result = complexity switch
        {
            1 => 18,
            2 => 30,
            3 => 44,
            4 => 59,
            5 => 66,
            6 => 71,
            7 => 100,
            8 => 130,
            9 => 190,
            10 => 254,
            _ => throw new ArgumentException("Not supported value of habit complexity")
        };
            
        return result;
    }

    public static double ToCoefficient( int complexity )
    {
        double coefOfComplexity = complexity switch
        {
            1 => 5.6,
            2 => 3.33,
            3 => 2.3,
            4 => 1.7,
            5 => 1.525,
            6 => 1.4,
            7 => 1.0,
            8 => 0.77,
            9 => 0.524,
            10 => 0.392,
            _ => throw new ArgumentException( message: $"Complexity {complexity} of habit is not supported",
                paramName: nameof( complexity ) )
        };
        
        return coefOfComplexity;
    }
}