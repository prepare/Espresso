using System;
using System.Collections.Generic;
using System.Reflection;
namespace Espresso.Extension
{
    public static class TypeExtention
    {
        public static bool ExtIsAssignableFrom(this Type a, Type b)
        {
#if NET20
            return a.IsAssignableFrom(b);
#else
            return a.GetTypeInfo().IsAssignableFrom(b.GetTypeInfo());
#endif

        }
        public static PropertyInfo ExtGetProperty(this Type a, object obj, string name)
        {
#if NET20
            BindingFlags flags;
            if (a == obj)
            {
                flags = BindingFlags.Public | BindingFlags.Static;
            }
            else
            {
                flags = BindingFlags.Public | BindingFlags.Instance;
            }
            return a.GetProperty(name, flags | BindingFlags.GetProperty);
#else
            
            return a.GetProperty(name);
#endif
        }
        public static FieldInfo ExtGetField(this Type a, object obj, string name)
        {
#if NET20
            BindingFlags flags;
            if (a == obj)
            {
                flags = BindingFlags.Public | BindingFlags.Static;
            }
            else
            {
                flags = BindingFlags.Public | BindingFlags.Instance;
            }
            return a.GetField(name, flags | BindingFlags.GetField);
#else
             
            return a.GetField(name );
#endif

        }
        public static void AddPublicMembers(this Type a, List<string> mbNameList)
        {
#if NET20
            foreach (var mb in a.GetMembers(BindingFlags.Public | BindingFlags.Instance))
            {
                var met = mb as MethodBase;
                if (met != null && !met.IsSpecialName)
                {
                    mbNameList.Add(mb.Name);
                }
            }

#else
            foreach (var mb in a.GetMembers())
            {
                var met = mb as MethodBase;
                if (met != null && !met.IsSpecialName)
                {
                    mbNameList.Add(mb.Name);
                }
            }

#endif

        }



        public static MemberInfo[] GetMembers(this Type type)
        {

#if NET20
            var members = type.GetMembers();
            List<MemberInfo> memList = new List<MemberInfo>();
            foreach (var mem in members)
            {
                memList.Add(mem);
            }
            return memList.ToArray();
#else
            var members = type.GetTypeInfo().DeclaredMembers;
            List<MemberInfo> memList = new List<MemberInfo>();
            foreach (var mem in members)
            {
                memList.Add(mem);
            }
            return memList.ToArray();
#endif
        }
    }
}

#if NET20
namespace System.Runtime.CompilerServices
{
    public class ExtensionAttribute : Attribute { }
}
#endif