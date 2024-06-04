using RimWorld;
using Verse;

namespace OberoniaAurea;

public class ThoughtWorker_LeaderOpinion : ThoughtWorker
{
	protected override ThoughtState CurrentSocialStateInternal(Pawn pawn, Pawn other)
	{
		Ideo ideo = pawn.Ideo;
		Faction faction = pawn.Faction;
		if (pawn.Spawned && ideo != null && faction != null && other == faction.leader && RelationsUtility.PawnsKnowEachOther(pawn, other))
		{
			if (ideo.HasPrecept(OADefOf.OA_RK_LeaderAttitude_Respect))
			{
				return ThoughtState.ActiveAtStage(0);
			}
			if (ideo.HasPrecept(OADefOf.OA_RK_LeaderAttitude_Worship))
			{
				return ThoughtState.ActiveAtStage(1);
			}
		}
		return ThoughtState.Inactive;
	}
}
