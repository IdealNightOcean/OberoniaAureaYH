using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace OberoniaAurea;

public class Gene_HotAdaptation : Gene
{
    public override void Tick()
    {
        if (pawn.IsHashIntervalTick(250))
        {
            TryAddHediff(pawn);
        }
    }
    private static void TryAddHediff(Pawn pawn)
    {
        if (!ModsConfig.BiotechActive)
        {
            return;
        }
        float ambientTemperature = pawn.AmbientTemperature;
        if (ambientTemperature > 45f)
        {
            OberoniaAureaYHUtility.AddHediff(pawn, OA_PawnInfoDefOf.OA_RK_Hediff_HotAdaptation, -1, 500);
        }
    }
}