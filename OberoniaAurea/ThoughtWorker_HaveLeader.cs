using RimWorld;
using Verse;

namespace OberoniaAurea;

public class ThoughtWorker_HaveLeader : ThoughtWorker
{
	protected override ThoughtState CurrentStateInternal(Pawn pawn)
	{
		Ideo ideo = pawn.Ideo;
		Faction faction = pawn.Faction;
		if (pawn.Spawned && ideo != null && faction != null)
		{
			if (faction.leader != null && !faction.leader.Dead)
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
			else
			{
				if (ideo.HasPrecept(OADefOf.OA_RK_LeaderAttitude_Respect))
				{
					return ThoughtState.ActiveAtStage(2);
				}
				if (ideo.HasPrecept(OADefOf.OA_RK_LeaderAttitude_Worship))
				{
					return ThoughtState.ActiveAtStage(3);
				}
			}
		}
		return ThoughtState.Inactive;
	}
}
