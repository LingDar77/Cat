namespace Cat.Utillities
{
    using System;
    using System.Collections;
    using System.Linq;
    using System.Reflection;
    public class InheritedFinder
    {
        public static Type[] FindInheritedTypes(Type parentType, Assembly assembly)
        {
            Type[] allTypes = assembly.GetTypes();
            ArrayList avTypesAL = new();

            return allTypes.Where(t => parentType.IsAssignableFrom(t) && t != parentType).ToArray();
        }
    }
}