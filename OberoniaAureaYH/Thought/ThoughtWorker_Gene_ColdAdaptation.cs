using RimWorld;
using UnityEngine;
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

        if (p.genes.HasGene(OberoniaAureaYHDefOf.OA_RK_Gene_ColdAdaptation))
        {
            float statValue = p.GetStatValue(StatDefOf.ComfyTemperatureMin);
            float ambientTemperature = p.AmbientTemperature;
            float adaptationTemperature = Mathf.Max(-30f, Mathf.Min(statValue + 20f, 0f));
            if (ambientTemperature < adaptationTemperature)
            {
                return ThoughtState.ActiveDefault;
            }
        }

        return ThoughtState.Inactive;
    }
}