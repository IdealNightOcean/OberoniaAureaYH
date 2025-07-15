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
        if (parms.target is Map map && map.GameConditionManager.ConditionIsActive(OARK_ModDefOf.OA_MilitaryDeployment))
        {
            ModUtility.CallForAidFixedPoints(map, ModUtility.OAFaction, 4500f);
        }
    }
}
