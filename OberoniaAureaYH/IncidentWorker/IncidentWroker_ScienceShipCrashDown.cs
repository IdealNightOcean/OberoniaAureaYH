using OberoniaAurea_Frame;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace OberoniaAurea;

public class IncidentWroker_ScienceShipCrashDown : IncidentWorker
{
    protected override bool CanFireNowSub(IncidentParms parms)
    {
        if (!ScienceDepartmentInteractHandler.IsInteractAvailable())
        {
            return false;
        }

        return parms.forced || ScienceDepartmentInteractHandler.Instance.ScienceShipRecord.HasValue;
    }

    protected override bool TryExecuteWorker(IncidentParms parms)
    {
        if (!CanFireNowSub(parms))
        {
            return false;
        }
        Map map = parms.target as Map;
        if (map is null)
        {
            if (Find.WorldObjects.AllWorldObjects.Where(wo => wo.def == OARK_WorldObjectDefOf.OARK_CrashedScienceShip).FirstOrFallback(null) is not MapParent_Enterable crashedScienceShip)
            {
                return false;
            }
            map = crashedScienceShip.Map;
            if (map is null)
            {
                return false;
            }
        }

        Thing scienceShip = ThingMaker.MakeThing(OARK_ThingDefOf.OARK_CrashedScienceShip);
        scienceShip.TryGetComp<CompCrashedScienceShip>().InitCrashedScienceShip();

        bool findLandPos = CellFinderLoose.TryFindSkyfallerCell(ThingDefOf.ShipChunkIncoming, map, ThingDefOf.ShipChunkIncoming.terrainAffordanceNeeded, out IntVec3 downPos, minDistToEdge: 50, nearLoc: map.Center, nearLocMaxDist: 200);
        downPos = findLandPos ? downPos : map.Center;

        Skyfaller crashShip = SkyfallerMaker.MakeSkyfaller(ThingDefOf.ShipChunkIncoming, scienceShip);
        GenSpawn.Spawn(crashShip, downPos, map);

        if (ScienceDepartmentInteractHandler.Instance.ScienceShipRecord.Value.TypeOfShip == ScienceShipRecord.ShipType.Fragile)
        {
            int popCount = Rand.RangeInclusive(4, 6);
            for (int i = 0; i < popCount; i++)
            {
                DropPop(downPos, map);
            }
        }

        return true;
    }

    private static readonly (ThingDef, IntRange)[] potentialThing =
        [
            (ThingDefOf.ComponentIndustrial,new IntRange(3,6)),
            (ThingDefOf.Plasteel,new IntRange(20,60)),
            (ThingDefOf.Uranium,new IntRange(20,60)),
            (ThingDefOf.ComponentSpacer,new IntRange(1,2)),
            (ThingDefOf.Gold,new IntRange(25,200)),
        ];

    private static void DropPop(IntVec3 centerCell, Map map)
    {
        CellFinderLoose.TryFindSkyfallerCell(ThingDefOf.DropPodIncoming, map, ThingDefOf.DropPodIncoming.terrainAffordanceNeeded, out IntVec3 downPos, minDistToEdge: 50, nearLoc: centerCell, nearLocMaxDist: 20);
        (ThingDef thingDef, IntRange countRange) = potentialThing.RandomElement();
        List<Thing> things = OAFrame_MiscUtility.TryGenerateThing(thingDef, countRange.RandomInRange);
        downPos = downPos.IsValid ? downPos : map.Center;
        DropPodUtility.DropThingsNear(downPos, map, things, openDelay: 10);
    }
}
