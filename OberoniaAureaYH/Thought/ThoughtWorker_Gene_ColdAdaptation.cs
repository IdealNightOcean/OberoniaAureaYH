using RimWorld;
using Verse;

namespace OberoniaAurea;

public class ThoughtWorker_Gene_ColdAdaptation : ThoughtWorker
{
    protected override ThoughtState CurrentStateInternal(Pawn p)
    {
        if (!ModsConfig.BiotechActive || p.genes == null)
        {
            return ThoughtState.Inactive;
        }

        if (p.genes.HasGene(OA_PawnInfoDefOf.OA_RK_Gene_ColdAdaptation))
        {
            float ambientTemperature = p.AmbientTemperature;
            if (ambientTemperature < -15f)
            {
                return ThoughtState.ActiveDefault;
            }
        }

        return ThoughtState.Inactive;
    }
}