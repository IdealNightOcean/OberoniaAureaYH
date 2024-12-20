using RimWorld;
using Verse;

namespace OberoniaAurea;

[StaticConstructorOnStartup]
public class ThoughtWorker_LeaderOpinion : ThoughtWorker
{
    protected override ThoughtState CurrentSocialStateInternal(Pawn pawn, Pawn other)
    {
        if (!pawn.Spawned || !other.Spawned)
        {
            return ThoughtState.Inactive;
        }
        Ideo ideo = pawn.Ideo;
        if (ideo != null && IsLeader(other, pawn.Faction) && RelationsUtility.PawnsKnowEachOther(pawn, other))
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
        return ThoughtState.Inactive;
    }

    protected static bool IsLeader(Pawn other, Faction faction)
    {
        if (faction == null || faction.leader == null)
        {
            return false;
        }
        return other == faction.leader;
    }
}
