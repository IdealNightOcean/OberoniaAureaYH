using HarmonyLib;
using System.Reflection;
using Verse;

namespace OberoniaAurea_Hyacinth;

[StaticConstructorOnStartup]
public static class ModHarmonyPatch
{
    private static Harmony harmonyInstance;
    internal static Harmony HarmonyInstance
    {
        get
        {
            harmonyInstance ??= new Harmony("OberoniaAurea.Hyacinth.Harmony");
            return harmonyInstance;
        }
    }

    static ModHarmonyPatch()
    {
        HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
    }
}

[StaticConstructorOnStartup]
public static class Hyacinth_Utility
{
    public static GameComponent_Hyacinth HyacinthGameComp => Current.Game.GetComponent<GameComponent_Hyacinth>();
}
