
namespace Elmah
{
    #region Imports

    using System.Globalization;

    #endregion

    /// <summary>
    /// Provides miscellaneous formatting methods for 
    /// </summary>

    public sealed class ErrorDisplay
    {
        /// <summary>
        /// Formats the type of an error, typically supplied as the 
        /// <see cref="Error.Type"/> value, in a short and human-
        /// readable form.
        /// </summary>
        /// <remarks>
        /// Typically, exception type names can be long to display and 
        /// complex to consume. The essential part can usually be found in
        /// the start of an exception type name minus its namespace. For
        /// example, a human reading the string,
        /// "System.Runtime.InteropServices.COMException", will usually
        /// considers "COM" as the most useful component of the entire
        /// type name. This method does exactly that. It assumes that the
        /// the input type is a .NET Framework exception type name where
        /// the namespace and class will be separated by the last 
        /// period (.) and where the type name ends in "Exception". If
        /// these conditions are method then a string like,
        /// "System.Web.HttpException" will be transformed into simply
        /// "Html".
        /// </remarks>

        public static string HumaneExceptionErrorType(string type)
        {
            if (type == null || type.Length == 0)
                return string.Empty;

            int lastDotIndex = CultureInfo.InvariantCulture.CompareInfo.LastIndexOf(type, '.');

            if (lastDotIndex > 0)
                type = type.Substring(lastDotIndex + 1);

            const string conventionalSuffix = "Exception";

            if (type.Length > conventionalSuffix.Length)
            {
                int suffixIndex = type.Length - conventionalSuffix.Length;

                if (string.Compare(type, suffixIndex, conventionalSuffix, 0,
                                   conventionalSuffix.Length, true, CultureInfo.InvariantCulture) == 0)
                {
                    type = type.Substring(0, suffixIndex);
                }
            }

            return type;
        }

        /// <summary>
        /// Formats the error type of an <see cref="Error"/> object in a 
        /// short and human-readable form.
        /// </summary>

        public static string HumaneExceptionErrorType(Error error)
        {
            if (error == null)
                throw new System.ArgumentNullException("error");

            return HumaneExceptionErrorType(error.Type);
        }

        private ErrorDisplay() { }
    }
}
