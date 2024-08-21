using OberoniaAurea_Frame;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace OberoniaAurea;

//金鸢尾兰慰问
public class IncidentWorker_OASupplyShipShowConcern : IncidentWorker
{
    public static Faction OAFaction => Find.FactionManager.FirstFactionOfDef(OberoniaAureaYHDefOf.OA_RK_Faction);
    protected bool TryResolveParmsGeneral(IncidentParms parms)
    {
        Faction oaFaction = OAFaction;
        if (oaFaction == null)
        {
            return false;
        }
        if (oaFaction.PlayerRelationKind != FactionRelationKind.Ally)
        {
            return false;
        }
        parms.faction = oaFaction;
        return true;
    }
    protected override bool TryExecuteWorker(IncidentParms parms)
    {
        if (!TryResolveParmsGeneral(parms))
        {
            return false;
        }
        Map map = (Map)parms.target;
        Faction faction = parms.faction;
        int count = map.mapPawns.FreeColonistsSpawnedCount * 100;
        IntVec3 intVec = DropCellFinder.TryFindSafeLandingSpotCloseToColony(map, IntVec2.One);
        ThingDef rawBerriesDef = DefDatabase<ThingDef>.GetNamed("RawBerries");
        List<List<Thing>> dropThings = OberoniaAureaFrameUtility.TryGengrateThingGroup(rawBerriesDef, count);
        foreach (List<Thing> things in dropThings)
            DropPodUtility.DropThingGroupsNear(intVec, map, [things], 110, leaveSlag: true, forbid: false, allowFogged: false, faction: faction);
        SendStandardLetter("OA_SupplyShipShowConcernLabel".Translate(), "OA_SupplyShipShowConcern".Translate(faction.Named("FACTION"), count), LetterDefOf.PositiveEvent, parms, new TargetInfo(intVec, map));
        return true;
    }
}
