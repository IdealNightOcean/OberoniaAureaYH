using OberoniaAurea_Frame;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace OberoniaAurea;

public class IncidentWorker_ScienceShip_OASupport : IncidentWorker_DropThingsOnMap
{
    protected override bool CanFireNowSub(IncidentParms parms)
    {
        return ScienceDepartmentInteractHandler.IsScienceDepartmentInteractAvailable() && base.CanFireNowSub(parms);
    }

    protected override bool ResolveMap(IncidentParms parms)
    {
        if (parms.target is not Map map)
        {
            if (Find.WorldObjects.AllWorldObjects.Where(wo => wo.def == OARK_WorldObjectDefOf.OARK_CrashedScienceShip).FirstOrFallback(null) is not MapParent_Enterable crashedScienceShip)
            {
                return false;
            }
            map = crashedScienceShip.Map;
            parms.target = map;
        }
        return map is not null;
    }

    protected override bool ResolveFaction(IncidentParms parms)
    {
        parms.faction ??= ModUtility.OAFaction;
        return parms.faction is not null;
    }

    protected override bool ResolveDropthings(IncidentParms parms)
    {
        List<Thing> items = OAFrame_MiscUtility.TryGenerateThing(ThingDefOf.Steel, 500);
        ThingDef targeterBombardmentDef = DefDatabase<ThingDef>.GetNamedSilentFail("OrbitalTargeterBombardment");
        if (targeterBombardmentDef is not null)
        {
            items.AddRange(OAFrame_MiscUtility.TryGenerateThing(targeterBombardmentDef, 2));
        }
        parms.gifts = items;
        return true;
    }

    protected override void PostThingDroped(IncidentParms parms)
    {
        Find.LetterStack.ReceiveLetter(label: "OARK_ScienceShip_OASupportLabel".Translate(),
                                       text: "OARK_ScienceShip_OASupport".Translate(),
                                       textLetterDef: LetterDefOf.PositiveEvent,
                                       lookTargets: new LookTargets(parms.spawnCenter, (Map)parms.target),
                                       relatedFaction: parms.faction);
    }
}
