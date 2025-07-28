using RimWorld;
using System.Linq;
using UnityEngine;
using Verse;

namespace OberoniaAurea;

public class CompProperties_GiveHediffInRange_Building : CompProperties
{
    public HediffDef hediff;

    public float range;

    public int checkInterval = 250;

    public CompProperties_GiveHediffInRange_Building()
    {
        compClass = typeof(CompGiveHediffInRange_Building);
    }
}

public class CompGiveHediffInRange_Building : ThingComp
{
    public CompProperties_GiveHediffInRange_Building Props => (CompProperties_GiveHediffInRange_Building)props;

    [Unsaved] protected CompPowerTrader powerTrader;

    protected int ticksToNextCheck;

    public override void PostExposeData()
    {
        Scribe_Values.Look(ref ticksToNextCheck, "ticksToNextCheck", 0);
    }

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        powerTrader = parent.TryGetComp<CompPowerTrader>();
    }

    public override void CompTickInterval(int delta)
    {
        if ((ticksToNextCheck -= delta) <= 0)
        {
            ticksToNextCheck = Props.checkInterval;
            if (powerTrader is not null && !powerTrader.PowerOn)
            {
                return;
            }
            HediffDef giveHediff = Props.hediff;
            int overrideDisappearTicks = Props.checkInterval + 120;
            Faction faction = parent.Faction;

            foreach (Pawn pawn in parent.Map.mapPawns.SpawnedPawnsInFaction(faction).Where(PawnValidator))
            {
                Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(giveHediff);
                if (hediff is null)
                {
                    hediff = pawn.health.AddHediff(giveHediff);
                    hediff.Severity = 1f;
                    HediffComp_Link hediffComp_Link = hediff.TryGetComp<HediffComp_Link>();
                    if (hediffComp_Link is not null)
                    {
                        hediffComp_Link.drawConnection = false;
                        hediffComp_Link.other = parent;
                    }
                }
                HediffComp_Disappears hediffComp_Disappears = hediff.TryGetComp<HediffComp_Disappears>();
                if (hediffComp_Disappears is null)
                {
                    Log.Error($"{nameof(parent)} has a hediff in props which does not have a HediffComp_Disappears");
                }
                else
                {
                    hediffComp_Disappears.ticksToDisappear = overrideDisappearTicks;
                }
            }
        }
    }

    protected bool PawnValidator(Pawn p)
    {
        return p.Position.DistanceTo(parent.Position) <= Props.range;
    }

    public override void PostDraw()
    {
        if (Find.Selector.SingleSelectedThing != parent || parent.Faction is null)
        {
            return;
        }

        Vector3 drawPos = parent.DrawPos;
        Faction faction = parent.Faction;
        foreach (Pawn pawn in parent.Map.mapPawns.SpawnedPawnsInFaction(faction).Where(PawnValidator))
        {
            GenDraw.DrawLineBetween(pawn.DrawPos, drawPos);
        }
    }
}
