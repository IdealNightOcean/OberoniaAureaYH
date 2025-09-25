using OberoniaAurea_Frame;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace OberoniaAurea;

//金鸢尾兰慰问
public class IncidentWorker_OASupplyShipShowConcern : IncidentWorker
{
    protected bool TryResolveParmsGeneral(IncidentParms parms)
    {
        Map map = (Map)parms.target;
        map ??= Find.AnyPlayerHomeMap;
        if (map is null)
        {
            return false;
        }
        parms.target = map;

        Faction oaFaction = ModUtility.OAFaction;
        if (oaFaction is null || oaFaction.PlayerRelationKind != FactionRelationKind.Ally)
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

        ThingDef rawBerriesDef = DefDatabase<ThingDef>.GetNamed("RawBerries");
        List<Thing> dropThings = OAFrame_MiscUtility.TryGenerateThing(rawBerriesDef, count);
        IntVec3 dropCell = OAFrame_DropPodUtility.DefaultDropThingGroups([dropThings], map, faction);
        SendStandardLetter(baseLetterLabel: "OARatkin_LetterLabel_SupplyShipShowConcern".Translate(),
                           baseLetterText: "OARatkin_Letter_SupplyShipShowConcern".Translate(faction.Named("FACTION"), count),
                           baseLetterDef: LetterDefOf.PositiveEvent,
                           parms: parms,
                           lookTargets: new TargetInfo(dropCell, map));
        return true;
    }
}
