namespace ElasticElmah.Core.Infrastructure
{
    #region Imports

    

    #endregion

    /// <summary>
    /// Collection of utility methods for masking values.
    /// </summary>
    public sealed class Mask
    {
        private Mask()
        {
        }

        public static string NullString(string s)
        {
            return s == null ? string.Empty : s;
        }

        public static string EmptyString(string s, string filler)
        {
            return NullString(s).Length == 0 ? filler : s;
        }
    }
}