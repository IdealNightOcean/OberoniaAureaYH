using RimWorld;
using Verse;

namespace OberoniaAurea_Hyacinth;

public class CompRatkinHarmTurret : ThingComp
{
    public CompProperties_RatkinHarmTurret Props => (CompProperties_RatkinHarmTurret)props;
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

    //绑定
    public override void Notify_Equipped(Pawn pawn)
    {
        RegisterRatkinHarmTurret(pawn);
    }
    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        RegisterRatkinHarmTurret(Wearer);
    }
    //解绑
    public override void Notify_Unequipped(Pawn pawn)
    {
        UnregisterRatkinHarmTurret(pawn);
    }
    public override void PostDeSpawn(Map map)
    {
        UnregisterRatkinHarmTurret(Wearer);
    }
    public override void PostDestroy(DestroyMode mode, Map previousMap)
    {
        UnregisterRatkinHarmTurret(Wearer);
    }
    protected void RegisterRatkinHarmTurret(Pawn pawn)
    {
        if (pawn == null)
        {
            return;
        }
        if (!Props.aiEffectiveUse && pawn.Faction != Faction.OfPlayer)
        {
            return;
        }
        Current.Game.GetComponent<GameComponent_Hyacinth>()?.RegisterRatkinHarmTurret(pawn, Props.RadiusSquared);
    }
    protected static void UnregisterRatkinHarmTurret(Pawn pawn)
    {
        Current.Game.GetComponent<GameComponent_Hyacinth>()?.UnregisterRatkinHarmTurret(pawn);
    }
}
