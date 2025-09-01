using OberoniaAurea_Frame;
using Verse;

namespace OberoniaAurea;

public class CompProperties_GravShield : CompProperties_EquipmentShield
{
    public HediffDef hediffOnActive;

    public CompProperties_GravShield()
    {
        compClass = typeof(CompGravShield);
    }
}

public class CompGravShield : CompEquipmentShield
{
    private new CompProperties_GravShield Props => (CompProperties_GravShield)props;

    protected override void Break()
    {
        base.Break();
        pawnOwner.RemoveFirstHediffOfDef(Props.hediffOnActive);
    }

    protected override void Reset()
    {
        base.Reset();
        pawnOwner.health.AddHediff(Props.hediffOnActive);
    }

    public override void Notify_Equipped(Pawn pawn)
    {
        base.Notify_Equipped(pawn);
        pawn.health.AddHediff(Props.hediffOnActive);
    }

    public override void Notify_Unequipped(Pawn pawn)
    {
        base.Notify_Unequipped(pawn);
        pawn.RemoveFirstHediffOfDef(Props.hediffOnActive);
    }
}