using OberoniaAurea_Frame;
using RimWorld;
using Verse;
using Verse.AI;

namespace OberoniaAurea;

public class Verb_TargetCellAid : Verb_CastBase
{
    protected override bool TryCastShot()
    {
        if (currentTarget.HasThing && currentTarget.Thing.Map != caster.Map)
        {
            return false;
        }
        Faction OAFaction = OberoniaAureaYHUtility.OAFaction;
        if (OAFaction == null || OAFaction.defeated || OAFaction.PlayerRelationKind == FactionRelationKind.Hostile)
        {
            return false;
        }
        Map map = caster.Map;
        IntVec3 cell = currentTarget.Cell;
        IncidentParms incidentParms = new()
        {
            target = map,
            faction = OAFaction,
            spawnCenter = cell,
            raidArrivalModeForQuickMilitaryAid = true,
            points = 1000f,
        };
        if (OAFrame_MiscUtility.TryFireIncidentNow(IncidentDefOf.RaidFriendly, incidentParms))
        {
            base.ReloadableCompSource?.UsedOnce();
            return true;
        }
        else
        {
            Log.Error(string.Concat("Could not send aid to map ", map, " from faction ", OAFaction));
            return false;
        }
    }
}

