using Verse;

namespace OberoniaAurea;

public class HediffCompProperties_TeaWithSkyWarmth : HediffCompProperties
{
    public HediffDef hediff;

    public float extraSeverity = 1f;

    public HediffCompProperties_TeaWithSkyWarmth()
    {
        compClass = typeof(HediffComp_TeaWithSkyWarmth);
    }
}


public class HediffComp_TeaWithSkyWarmth : HediffComp
{
    public HediffCompProperties_TeaWithSkyWarmth Props => (HediffCompProperties_TeaWithSkyWarmth)props;

    public override void CompPostPostAdd(DamageInfo? dinfo)
    {
        Hediff firstHediffOfDef = parent.pawn.health.hediffSet.GetFirstHediffOfDef(Props.hediff);
        if (firstHediffOfDef is not null)
        {
            firstHediffOfDef.Severity = Props.extraSeverity;
        }
    }
    public override void CompPostPostRemoved()
    {
        Hediff firstHediffOfDef = parent.pawn.health.hediffSet.GetFirstHediffOfDef(Props.hediff);
        if (firstHediffOfDef is not null)
        {
            firstHediffOfDef.Severity = 1f;
        }
    }
}
