using RimWorld;
using Verse;

namespace OberoniaAurea;

public class IncidentWorker_DropThingsOnMap : IncidentWorker
{
    protected override bool CanFireNowSub(IncidentParms parms)
    {
        return ResolveMap(parms) && ResolveFaction(parms);
    }

    protected virtual bool ResolveMap(IncidentParms parms)
    {
        if (parms.target is not Map map)
        {
            map = Find.AnyPlayerHomeMap;
            parms.target = map;
        }

        return map is not null;
    }

    protected virtual bool ResolveFaction(IncidentParms parms)
    {
        return true;
    }

    protected virtual bool ResolveDropthings(IncidentParms parms)
    {
        return !parms.gifts.NullOrEmpty();
    }

    protected virtual void PostThingDroped(IncidentParms parms) { }

    protected override bool TryExecuteWorker(IncidentParms parms)
    {
        if (!CanFireNowSub(parms))
        {
            return false;
        }

        Map map = (Map)parms.target;
        if (ResolveDropthings(parms))
        {
            parms.spawnCenter = OARK_DropPodUtility.DefaultDropThingGroups([parms.gifts], map, parms.faction);
            PostThingDroped(parms);
            return true;
        }
        else
        {
            return false;
        }
    }
}