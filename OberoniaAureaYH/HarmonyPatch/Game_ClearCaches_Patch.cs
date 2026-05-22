using HarmonyLib;
using Verse;

namespace OberoniaAurea;

[StaticConstructorOnStartup]
[HarmonyPatch(typeof(Game), nameof(Game.ClearCaches))]
public static class Game_ClearCaches_Patch
{
    [HarmonyPostfix]
    public static void Postfix()
    {
        ModUtility.ClearStaticCache();

        GameComponent_OberoniaAurea.ClearStaticCache();

        ScienceDepartmentDialogUtility.ClearStaticCache();
        ThoughtWorker_Precept_Snow.ClearStaticCache();
        ThoughtWorker_SpaceTraveler.ClearStaticCache();
    }
}
