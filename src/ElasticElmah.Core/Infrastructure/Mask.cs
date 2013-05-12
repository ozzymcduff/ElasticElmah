namespace Elmah
{
    #region Imports

    using System;

    #endregion

    /// <summary>
    /// Collection of utility methods for masking values.
    /// </summary>
    
    public sealed class Mask
    {
        public static string NullString(string s)
        {
            return s == null ? string.Empty : s;
        }

        public static string EmptyString(string s, string filler)
        {
            return Mask.NullString(s).Length == 0 ? filler : s;
        }
        
        private Mask() {}
    }
}
