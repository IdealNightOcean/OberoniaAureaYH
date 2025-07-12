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
        if (ideo is not null && IsLeader(other, pawn.Faction) && RelationsUtility.PawnsKnowEachOther(pawn, other))
        {
            if (ideo.HasPrecept(OARatkin_PreceptDefOf.OA_RK_LeaderAttitude_Respect))
            {
                return ThoughtState.ActiveAtStage(0);
            }
            else if (ideo.HasPrecept(OARatkin_PreceptDefOf.OA_RK_LeaderAttitude_Worship))
            {
                return ThoughtState.ActiveAtStage(1);
            }
        }
        return ThoughtState.Inactive;
    }

    protected static bool IsLeader(Pawn other, Faction faction)
    {
        if (faction is null || faction.leader is null)
        {
            return false;
        }
        return other == faction.leader;
    }
}
