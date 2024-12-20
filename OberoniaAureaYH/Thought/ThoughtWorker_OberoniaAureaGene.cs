using RimWorld;
using Verse;

namespace OberoniaAurea;

public class ThoughtWorker_OberoniaAureaGene : ThoughtWorker
{
    protected override ThoughtState CurrentStateInternal(Pawn p)
    {
        if (!p.Spawned)
        {
            return ThoughtState.Inactive;
        }
        int count = p.Map.GetOAMapComp()?.OAGenePawnsCount ?? 0;
        if (count <= 2)
        {
            return ThoughtState.ActiveAtStage(0);
        }
        if (count <= 5)
        {
            return ThoughtState.ActiveAtStage(1);
        }
        if (count <= 10)
        {
            return ThoughtState.Inactive;
        }
        if (count <= 25)
        {
            return ThoughtState.ActiveAtStage(2);
        }
        return ThoughtState.ActiveAtStage(3);
    }
}
