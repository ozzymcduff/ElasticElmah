
namespace Elmah
{
    #region Imports

    using System;
    using System.Collections;
    using System.Globalization;

    #endregion

    /// <summary> 
    /// Helper methods for array containing culturally-invariant strings.
    /// The main reason for this helper is to help with po
    /// </summary>
 
    public sealed class InvariantStringArray
    {
        public static void Sort(string[] keys)
        {
            Sort(keys, 0, keys.Length);
        }

        public static void Sort(string[] keys, int index, int length)
        {
            Sort(keys, null, index, length);
        }
        
        public static void Sort(string[] keys, Array items, int index, int length)
        {
            Array.Sort(keys, items, index, length, InvariantComparer);
        }

        private static IComparer InvariantComparer
        {
            get
            {
                return Comparer.DefaultInvariant;
            }
        }

        private InvariantStringArray() {}
    }
}