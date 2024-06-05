using RimWorld;
using System.Reflection;
using Verse;

namespace OberoniaAurea;
public class HediffCompProperties_ExtraStun : HediffCompProperties
{
    public int extraStunTicks = 300;

    public int triggerDelay = 60;

    public HediffCompProperties_ExtraStun()
    {
        compClass = typeof(HeiffComp_ExtraStun);
    }
}
public class HeiffComp_ExtraStun : HediffComp
{
    public int ticksRemaining = 60;
    HediffCompProperties_ExtraStun Props => props as HediffCompProperties_ExtraStun;
    public override void CompPostPostAdd(DamageInfo? dinfo)
    {
        if (Pawn.RaceProps.IsFlesh)
        {
            Pawn.health.RemoveHediff(parent);
        }
        else
        {
            ticksRemaining = Props.triggerDelay;
        }
    }
    public override void CompPostTick(ref float severityAdjustment)
    {
        ticksRemaining--;
        if (ticksRemaining < 0)
        {
            ExtraStun(Pawn, Props.extraStunTicks);
            Pawn.health.RemoveHediff(parent);
        }
    }
    protected static void ExtraStun(Pawn pawn, int extraStunTicks)
    {
        StunHandler stunHandler = pawn.stances.stunner;
        if (stunHandler.StunTicksLeft > 0)
        {
            int totalStunTicks = stunHandler.StunTicksLeft + extraStunTicks;
            stunHandler.GetType().GetField("stunTicksLeft", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(stunHandler, totalStunTicks);
        }
        else
        {
            stunHandler.StunFor(extraStunTicks, null, addBattleLog: false);
        }
    }
    public override void CompExposeData()
    {
        Scribe_Values.Look(ref ticksRemaining, "ticksRemaining", 0);
    }
}
