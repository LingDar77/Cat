using System;
using System.Linq;
using System.Reflection;

namespace Cat.Utilities
{
    public static class ReflectExpand
    {
        public static FieldInfo[] GetFieldsWithAttribute<AttributeType>(this Type type)
        {
            return type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public).Where(i => i.GetCustomAttribute(typeof(AttributeType), false) != null).ToArray();
        }
        public static PropertyInfo[] GetPropertiesWithAttribute<AttributeType>(this Type type)
        {
            return type.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public).Where(i => i.GetCustomAttribute(typeof(AttributeType), false) != null).ToArray();
        }
    }
}