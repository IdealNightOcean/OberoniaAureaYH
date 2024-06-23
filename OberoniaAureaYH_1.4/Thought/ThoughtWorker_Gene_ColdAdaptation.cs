using RimWorld;
using Verse;

namespace OberoniaAurea;

public class ThoughtWorker_Gene_ColdAdaptation : ThoughtWorker
{
    protected override ThoughtState CurrentStateInternal(Pawn p)
    {
        if (!ModsConfig.BiotechActive)
        {
            return ThoughtState.Inactive;
        }
        float ambientTemperature = p.AmbientTemperature;
        if (ambientTemperature < -15f)
        {
            return ThoughtState.ActiveDefault;
        }
        return ThoughtState.Inactive;
    }
}