using HarmonyLib;
using RimWorld;
using Verse;

namespace OberoniaAurea;

//派系关系改变Patch - 记录金鸢尾兰结盟时刻
[StaticConstructorOnStartup]
[HarmonyPatch(typeof(Faction), "Notify_RelationKindChanged")]
public static class Notify_RelationKindChanged_Patch
{
    [HarmonyPostfix]
    public static void Postfix(ref Faction __instance, Faction other)
    {
        if (__instance.def != OARK_ModDefOf.OA_RK_Faction || !other.IsPlayerSafe())
        {
            return;
        }
        FactionRelationKind curRelation = __instance.PlayerRelationKind;
        OAInteractHandler interactHandler = OAInteractHandler.Instance;
        switch (curRelation)
        {
            case FactionRelationKind.Ally:
                interactHandler.CurAllianceInitTick = Find.TickManager.TicksGame;
                return;
            case FactionRelationKind.Neutral:
                interactHandler.OldAllianceDuration = interactHandler.AllianceDuration();
                interactHandler.CurAllianceInitTick = -1;
                return;
            case FactionRelationKind.Hostile:
                interactHandler.OldAllianceDuration = 0f;
                interactHandler.CurAllianceInitTick = -1;
                return;
            default: return;
        }
    }
}
