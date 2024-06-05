using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace OberoniaAurea;

public class RaidStrategyWorker_OADropAttack : RaidStrategyWorker_ImmediateAttack
{
	protected override LordJob MakeLordJob(IncidentParms parms, Map map, List<Pawn> pawns, int raidSeed)
	{
		IntVec3 intVec = (parms.spawnCenter.IsValid ? parms.spawnCenter : pawns[0].PositionHeld);
		Thing thing = ThingMaker.MakeThing(OADefOf.OA_RK_EMPInst);
		DropPodUtility.DropThingsNear(intVec, map, new _003C_003Ez__ReadOnlyArray<Thing>(new Thing[1] { thing }));
		if (parms.attackTargets != null && parms.attackTargets.Count > 0)
		{
			return new LordJob_AssaultThings(parms.faction, parms.attackTargets);
		}
		if (parms.faction.HostileTo(Faction.OfPlayer))
		{
			return new LordJob_AssaultColony(parms.faction, canKidnap: true, parms.canTimeoutOrFlee);
		}
		RCellFinder.TryFindRandomSpotJustOutsideColony(intVec, map, out var result);
		return new LordJob_AssistColony(parms.faction, result);
	}
}
