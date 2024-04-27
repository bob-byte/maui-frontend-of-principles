namespace SET.Core.Extensions;

public static class DictionaryExtensions
{
    public static bool ValueAsBool( this IDictionary<string, object> dictionary, string key, bool defaultValue = false )
    {
        return dictionary.ContainsKey( key ) && dictionary[key] is bool dictValue
            ? dictValue
            : defaultValue;
    }

    public static int ValueAsInt( this IDictionary<string, object> dictionary, string key, int defaultValue = 0 )
    {
        return dictionary.ContainsKey( key ) && dictionary[key] is int intValue
            ? intValue
            : defaultValue;
    }
}
