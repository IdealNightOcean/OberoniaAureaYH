using RimWorld;
using Verse;

namespace OberoniaAurea_Hyacinth;

public class CompAntiStealth : ThingComp
{
    public CompProperties_AntiStealth Props => (CompProperties_AntiStealth)props;
    public Pawn Wearer
    {
        get
        {
            if (base.ParentHolder is not Pawn_ApparelTracker pawn_ApparelTracker)
            {
                return null;
            }
            return pawn_ApparelTracker.pawn;
        }
    }
    //°ó¶¨
    public override void Notify_Equipped(Pawn pawn)
    {
        RegisterAntiStealth(pawn);
    }
    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        RegisterAntiStealth(Wearer);
    }
    //½â°ó
    public override void Notify_Unequipped(Pawn pawn)
    {
        UnregisterAntiStealth(pawn);
    }
    public override void PostDeSpawn(Map map)
    {
        UnregisterAntiStealth(Wearer);
    }
    public override void PostDestroy(DestroyMode mode, Map previousMap)
    {
        UnregisterAntiStealth(Wearer);
    }
    protected void RegisterAntiStealth(Pawn pawn)
    {
        if (pawn == null)
        {
            return;
        }
        if (!Props.aiEffectiveUse && pawn.Faction != Faction.OfPlayer)
        {
            return;
        }
        Current.Game.GetComponent<GameComponent_Hyacinth>()?.RegisterAntiStealth(pawn, Props.RadiusSquared);
    }
    protected static void UnregisterAntiStealth(Pawn pawn)
    {
        Current.Game.GetComponent<GameComponent_Hyacinth>()?.UnregisterAntiStealth(pawn);
    }

    
}
