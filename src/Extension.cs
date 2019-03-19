//MIT, 2015-present, WinterDev, EngineKit, brezza92
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
        public static Type ExtGetInnerTypeIfNullableValue(this Type type)
        {
            //TODO: review here again
#if NET20
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                return type.GetGenericArguments()[0];
            return null;
#else
            TypeInfo typeinfo = type.GetTypeInfo();
             if (typeinfo.IsGenericType && typeinfo.GetGenericTypeDefinition() == typeof(Nullable<>))
                return typeinfo.GenericTypeArguments[0];                 
            return null;
#endif
        }
        public static PropertyInfo ExtGetProperty(this Type a, object obj, string name)
        {
#if NET20
            BindingFlags flags;
            if (obj == null)
            {
                flags = BindingFlags.Public | BindingFlags.Static;
            }
            else
            {
                flags = BindingFlags.Public | BindingFlags.Instance;
            }
            return a.GetProperty(name, flags | BindingFlags.GetProperty);
#else
            
            return a.GetRuntimeProperty(name);
#endif
        }
        public static PropertyInfo ExtGetPropertySetter(this Type a, object obj, string name)
        {
#if NET20
            BindingFlags flags;
            if (obj == null)
            {
                flags = BindingFlags.Public | BindingFlags.Static;
            }
            else
            {
                flags = BindingFlags.Public | BindingFlags.Instance;
            }
            return a.GetProperty(name, flags | BindingFlags.SetProperty);
#else
            return a.GetRuntimeProperty(name);      
#endif
        }
        public static FieldInfo ExtGetField(this Type a, object obj, string name)
        {
#if NET20
            BindingFlags flags;
            if (obj == null)
            {
                flags = BindingFlags.Public | BindingFlags.Static;
            }
            else
            {
                flags = BindingFlags.Public | BindingFlags.Instance;
            }

            return a.GetField(name, flags | BindingFlags.GetField);
#else
             
            return a.GetRuntimeField(name);
             
#endif

        }

        public static MethodInfo ExtGetMethod(this Type a, object obj, string name)
        {
            //TODO review this again

#if NET20
            BindingFlags flags;
            if (obj == null)
            {
                flags = BindingFlags.Public | BindingFlags.Static;
            }
            else
            {
                flags = BindingFlags.Public | BindingFlags.Instance;
            }
            BindingFlags mFlags = flags | BindingFlags.InvokeMethod | BindingFlags.FlattenHierarchy;
            foreach (MethodInfo met in a.GetMethods(mFlags))
            {
                if (met.Name == name)
                {
                    return met;
                }
            }

#else
     
            foreach(MemberInfo mb in a.GetMembers())
            {
                MethodInfo met = mb as MethodInfo;
                if(met != null && met.Name== name)
                {
                    return met;
                }
            }

             
#endif
            return null;
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