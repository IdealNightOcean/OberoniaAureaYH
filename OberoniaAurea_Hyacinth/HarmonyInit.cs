using HarmonyLib;
using JetBrains.Annotations;
using System.Reflection;
using Verse;

namespace OberoniaAurea_Hyacinth;

[UsedImplicitly]
[StaticConstructorOnStartup]
public class HarmonyInit
{
    private static Harmony harmonyInstance;

    internal static Harmony HarmonyInstance
    {
        get
        {
            harmonyInstance ??= new Harmony("fxz.jywl.patch");
            return harmonyInstance;
        }
    }

    static HarmonyInit()
    {
        HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
    }
}
