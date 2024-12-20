using RimWorld;
using Verse;

namespace OberoniaAurea;

public class CompProperties_GiveTargeterHediff : CompProperties_UseEffect
{
    public HediffDef hediff;

    public CompProperties_GiveTargeterHediff()
    {
        compClass = typeof(CompTargetEffect_GiveTargeterHediff);
    }
}

public class CompTargetEffect_GiveTargeterHediff : CompTargetEffect
{
    public CompProperties_GiveTargeterHediff Props => (CompProperties_GiveTargeterHediff)props;

    public override void DoEffectOn(Pawn user, Thing target)
    {
        if (target is Pawn targetPawn && !targetPawn.Dead)
        {
            Hediff hediff = HediffMaker.MakeHediff(Props.hediff, targetPawn);
            targetPawn.health.AddHediff(hediff);
        }
    }
}
