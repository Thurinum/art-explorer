global using OneOf;

public static class OneOfExtensions
{
    // Alias extension methods for OneOf<T1, T2>
    // Because IsT0 and IsT1 are a bit confusing lol
    public static bool IsOk<T1, T2>(this OneOf<T1, T2> oneOf) => oneOf.IsT0;
    public static bool IsError<T1, T2>(this OneOf<T1, T2> oneOf) => oneOf.IsT1;
    public static T1 Result<T1, T2>(this OneOf<T1, T2> oneOf) => oneOf.AsT0;
    public static T2 Error<T1, T2>(this OneOf<T1, T2> oneOf) => oneOf.AsT1;
}