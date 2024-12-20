using RimWorld;
using Verse;

namespace OberoniaAurea;

public class ThoughtWorker_HaveAliveLeader : ThoughtWorker
{
    protected override ThoughtState CurrentStateInternal(Pawn pawn)
    {
        if (!pawn.Spawned)
        {
            return ThoughtState.Inactive;
        }
        Ideo ideo = pawn.Ideo;
        Faction faction = pawn.Faction;
        if (ideo != null && faction != null)
        {
            if (faction.leader != null && !faction.leader.Dead)
            {
                if (ideo.HasPrecept(OA_PreceptDefOf.OA_RK_LeaderAttitude_Respect))
                {
                    return ThoughtState.ActiveAtStage(0);
                }
                else if (ideo.HasPrecept(OA_PreceptDefOf.OA_RK_LeaderAttitude_Worship))
                {
                    return ThoughtState.ActiveAtStage(1);
                }
            }
            else
            {
                if (ideo.HasPrecept(OA_PreceptDefOf.OA_RK_LeaderAttitude_Respect))
                {
                    return ThoughtState.ActiveAtStage(2);
                }
                else if (ideo.HasPrecept(OA_PreceptDefOf.OA_RK_LeaderAttitude_Worship))
                {
                    return ThoughtState.ActiveAtStage(3);
                }
            }
        }
        return ThoughtState.Inactive;
    }
}
