using Verse;

namespace OberoniaAurea;

public class Gene_HotAdaptation : Gene
{
    private int tickRemaining;
    public override void Tick()
    {
        tickRemaining--;
        if (tickRemaining < 0)
        {
            TryAddHediff(pawn);
            tickRemaining = 250;
        }
    }
    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref tickRemaining, "tickRemaining", 0);
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