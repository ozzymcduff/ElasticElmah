using System;

namespace ElasticElmah.Appender.Tests
{
    public static class Extensions
    {
        public static T Tap<T>(this T that, Action<T> tapaction)
        {
            tapaction(that);
            return that;
        }
    }
}
