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
        if (__instance.def != OA_MiscDefOf.OA_RK_Faction || other != Faction.OfPlayer)
        {
            return;
        }
        FactionRelationKind curRelation = __instance.RelationKindWith(Faction.OfPlayer);
        GameComponent_OberoniaAurea oaGameComp = OA_MiscUtility.OAGameComp;
        switch (curRelation)
        {
            case FactionRelationKind.Ally:
                oaGameComp.initAllianceTick = Find.TickManager.TicksGame;
                return;
            case FactionRelationKind.Neutral:
                oaGameComp.OldAllianceDuration = oaGameComp.AllianceDuration;
                oaGameComp.initAllianceTick = -1;
                return;
            case FactionRelationKind.Hostile:
                oaGameComp.OldAllianceDuration = 0f;
                oaGameComp.initAllianceTick = -1;
                return;
            default: return;
        }
    }
}
