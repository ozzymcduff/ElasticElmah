using System;

namespace ElasticElmah.Appender.Tests
{
    internal static class Extensions
    {
        public static T Tap<T>(this T that, Action<T> tapaction)
        {
            tapaction(that);
            return that;
        }
        public static T TapNotNull<T>(this T that, Action<T> tapaction) where T : class
        {
            if (that != null)
                tapaction(that);
            return that;
        }
    }
}