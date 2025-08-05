using Verse;

namespace OberoniaAurea;
public class HediffCompProperties_GiveHediffWhenRemoved : HediffCompProperties
{
    public HediffDef hediffDef;

    public bool skipIfAlreadyExists;

    public HediffCompProperties_GiveHediffWhenRemoved()
    {
        compClass = typeof(HediffComp_GiveHediffWhenRemoved);
    }
}
public class HediffComp_GiveHediffWhenRemoved : HediffComp
{
    private HediffCompProperties_GiveHediffWhenRemoved Props => (HediffCompProperties_GiveHediffWhenRemoved)props;

    public override void CompPostPostRemoved()
    {
        base.CompPostPostRemoved();
        if (!Props.skipIfAlreadyExists || !parent.pawn.health.hediffSet.HasHediff(Props.hediffDef))
        {
            parent.pawn.health.AddHediff(Props.hediffDef);
        }
    }

}
