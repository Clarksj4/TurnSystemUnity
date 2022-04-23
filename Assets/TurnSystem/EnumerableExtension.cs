using System.Collections.Generic;
using UnityEngine;

namespace TurnBased
{
    public static class EnumerableExtension
    {
        public static LinkedList<T> ToLinkedList<T>(this IEnumerable<T> enumerable)
        {
            return new LinkedList<T>(enumerable);
        }
    }
}