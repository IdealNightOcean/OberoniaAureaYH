using RimWorld;
using UnityEngine;
using Verse;

namespace OberoniaAurea;

public class ThoughtWorker_Gene_HotAdaptation : ThoughtWorker
{
    protected override ThoughtState CurrentStateInternal(Pawn p)
    {
        if (!ModsConfig.BiotechActive || p.genes == null)
        {
            return ThoughtState.Inactive;
        }

        if (p.genes.HasGene(OA_PawnInfoDefOf.OA_RK_Gene_HotAdaptation))
        {
            float statValue = p.GetStatValue(StatDefOf.ComfyTemperatureMax);
            float ambientTemperature = p.AmbientTemperature;
            float adaptationTemperature = Mathf.Min(50f, Mathf.Max(statValue + 20f, 30f));
            if (ambientTemperature > adaptationTemperature)
            {
                return ThoughtState.ActiveDefault;
            }
        }

        return ThoughtState.Inactive;
    }
}