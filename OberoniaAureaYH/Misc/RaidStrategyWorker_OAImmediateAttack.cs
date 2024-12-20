using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace OberoniaAurea;

public class RaidStrategyWorker_OAImmediateAttack : RaidStrategyWorker_ImmediateAttack
{
    public override bool CanUseWith(IncidentParms parms, PawnGroupKindDef groupKind)
    {
        if (!base.CanUseWith(parms, groupKind))
        {
            return false;
        }
        if (parms.faction != null)
        {
            return parms.faction.IsOAFaction();
        }
        return false;
    }
    protected override LordJob MakeLordJob(IncidentParms parms, Map map, List<Pawn> pawns, int raidSeed)
    {
        IntVec3 intVec = (parms.spawnCenter.IsValid ? parms.spawnCenter : pawns[0].PositionHeld);
        Thing dropThing = ThingMaker.MakeThing(OA_ThingDefOf.OA_RK_EMPInst);
        DropPodUtility.DropThingsNear(intVec, map, [dropThing]);
        return base.MakeLordJob(parms, map, pawns, raidSeed);
    }
}
public class RaidStrategyWorker_OAImmediateAttackFriendly : RaidStrategyWorker_OAImmediateAttack
{
    public override bool CanUseWith(IncidentParms parms, PawnGroupKindDef groupKind)
    {
        if (!base.CanUseWith(parms, groupKind))
        {
            return false;
        }
        if (parms.faction != null)
        {
            return !parms.faction.HostileTo(Faction.OfPlayer);
        }
        return false;
    }
}