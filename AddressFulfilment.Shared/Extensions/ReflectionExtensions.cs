using System;
using System.Linq;
using System.Reflection;

namespace AddressFulfilment.Shared.Extensions
{
    public static class ReflectionExtensions
    {
        public static bool IsPropertyWithSetter(this MemberInfo member)
        {
            var property = member as PropertyInfo;

            return property?.GetSetMethod(true) != null;
        }

        public static bool HasAttribute<TAttribute>(this MemberInfo method) where TAttribute : Attribute
        {
            return method.GetCustomAttributes(typeof(TAttribute), false).Any();
        }
    }
}