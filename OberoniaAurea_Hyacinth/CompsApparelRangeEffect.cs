using RimWorld;
using Verse;

namespace OberoniaAurea_Hyacinth;
public class CompProperties_ApparelRangeEffect : CompProperties
{
    public bool aiEffectiveUse;
    public float radius;
    public float RadiusSquared => radius * radius;
}

public abstract class CompsApparelRangeEffect : ThingComp
{
    public CompProperties_ApparelRangeEffect Props => (CompProperties_ApparelRangeEffect)props;
    public Pawn Wearer => (parent as Apparel)?.Wearer;

    //绑定
    public override void Notify_Equipped(Pawn pawn)
    {
        RegisterGameComp(pawn);
    }
    //解绑
    public override void Notify_Unequipped(Pawn pawn)
    {
        UnregisterGameComp(pawn);
    }
    public override void PostDestroy(DestroyMode mode, Map previousMap)
    {
        UnregisterGameComp(Wearer);
    }
    protected abstract void RegisterGameComp(Pawn pawn);
    protected abstract void UnregisterGameComp(Pawn pawn);
}