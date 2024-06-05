using System;

namespace vcrossing.Code.Helpers;

public static class ListExtensions
{

    public static void ForEach<T>( this List<T> list, Action<T> action )
    {
        foreach ( var item in list )
        {
            action( item );
        }
    }

    /// <summary>
    /// Pick a random item from the list.
    /// </summary>
    public static T PickRandom<T>( this List<T> list )
    {
        var random = new Random();
        return list[random.Next( list.Count )];
    }

}
