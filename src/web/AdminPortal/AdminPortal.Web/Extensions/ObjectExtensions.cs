using System;

namespace Laso.AdminPortal.Web.Extensions
{
    internal static class ObjectExtensions
    {
        public static T With<T>(this T instance, Action<T> action)
        {
            action(instance);

            return instance;
        }
    }
}