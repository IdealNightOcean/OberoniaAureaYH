﻿using System.Reflection;
using Verse;

namespace OberoniaAurea;

[StaticConstructorOnStartup]
public static class ReflectionUtility
{
    public static BindingFlags BindingAttr = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    public static T GetFieldValue<T>(object obj, string name, T fallback)
    {
        object obj2 = (obj?.GetType().GetField(name, BindingAttr))?.GetValue(obj);
        if (obj2 != null)
        {
            return (T)obj2;
        }
        return fallback;
    }
    public static void SetFieldValue(object obj, string name, object value)
    {
        (obj?.GetType().GetField(name, BindingAttr))?.SetValue(obj, value);
    }
}