using HarmonyLib;
using RimWorld;
using Verse;

namespace OberoniaAurea;
//军事部署 - 袭击时立刻支援Patch
[StaticConstructorOnStartup]
[HarmonyPatch(typeof(IncidentWorker_RaidEnemy), "TryExecuteWorker")]
public static class TryExecuteWorker_Patch
{
    [HarmonyPostfix]
    public static void Postfix(ref bool __result, IncidentParms parms)
    {
        if (!__result)
        {
            return;
        }
        if (parms.target is Map map && map.GameConditionManager.ConditionIsActive(OARatkin_MiscDefOf.OA_MilitaryDeployment))
        {
            OARatkin_MiscUtility.CallForAidFixedPoints(map, OARatkin_MiscUtility.OAFaction, 4500f);
        }
    }
}
