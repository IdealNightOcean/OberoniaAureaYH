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
        if (__instance.def != OberoniaAureaYHDefOf.OA_RK_Faction || other != Faction.OfPlayer)
        {
            return;
        }
        FactionRelationKind curRelation = __instance.RelationKindWith(Faction.OfPlayer);
        GameComponent_OberoniaAurea gc_OA = OberoniaAureaYHUtility.OA_GCOA;
        switch (curRelation)
        {
            case FactionRelationKind.Ally:
                gc_OA.initAllianceTick = Find.TickManager.TicksGame;
                return;
            case FactionRelationKind.Neutral:
                gc_OA.OldAllianceDuration = gc_OA.AllianceDuration;
                gc_OA.initAllianceTick = -1;
                return;
            case FactionRelationKind.Hostile:
                gc_OA.OldAllianceDuration = 0f;
                gc_OA.initAllianceTick = -1;
                return;
            default: return;
        }
    }
}
