using RimWorld;
using Verse;

namespace OberoniaAurea;

[StaticConstructorOnStartup]
public class ThoughtWorker_LeaderDowned : ThoughtWorker
{
    protected override ThoughtState CurrentStateInternal(Pawn pawn)
    {
        if (!pawn.Spawned)
        {
            return ThoughtState.Inactive;
        }
        Ideo ideo = pawn.Ideo;
        if (ideo is not null && LeaderDowned(pawn.Faction))
        {
            if (ideo.HasPrecept(OARK_PreceptDefOf.OA_RK_LeaderAttitude_Respect))
            {
                return ThoughtState.ActiveAtStage(0);
            }
            else if (ideo.HasPrecept(OARK_PreceptDefOf.OA_RK_LeaderAttitude_Worship))
            {
                return ThoughtState.ActiveAtStage(1);
            }
        }
        return ThoughtState.Inactive;
    }

    protected static bool LeaderDowned(Faction faction)
    {
        if (faction is null || faction.leader is null)
        {
            return false;
        }
        return faction.leader.Downed;
    }
}
