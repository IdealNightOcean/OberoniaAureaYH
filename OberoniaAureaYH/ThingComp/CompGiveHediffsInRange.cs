using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace OberoniaAurea;

public class CompProperties_GiveHediffsInRange : CompProperties
{
    public float range;
    public TargetingParameters targetingParameters;
    public HediffDef hediff;
    public ThingDef mote;
    public bool hideMoteWhenNotDrafted;
    public float initialSeverity = 1f;
    public bool canApplyOnSelf = true;
    public bool onlyPlayerCanUse;
    public bool onlyPawnsInSameFaction = true;
    public int checkInterval = 250;
    public int hediffDisappearTick = 500;

    public CompProperties_GiveHediffsInRange()
    {
        compClass = typeof(CompGiveHediffsInRange);
    }
}

public class CompGiveHediffsInRange : ThingComp
{
    private Mote mote;

    public CompProperties_GiveHediffsInRange Props => (CompProperties_GiveHediffsInRange)props;
    public Apparel ApparelParent => parent as Apparel;

    protected Pawn Wearer => ApparelParent.Wearer;

    public override void CompTick()
    {
        Pawn wearer = Wearer;
        if (wearer is null || !wearer.Spawned)
        {
            return;
        }
        if (Props.onlyPlayerCanUse && !wearer.Faction.IsPlayerSafe())
        {
            return;
        }
        if (!Props.hideMoteWhenNotDrafted || wearer.Drafted)
        {
            if (Props.mote is not null && (mote is null || mote.Destroyed))
            {
                mote = MoteMaker.MakeAttachedOverlay(wearer, Props.mote, Vector3.zero);
            }
            mote?.Maintain();
        }
        if (!parent.IsHashIntervalTick(Props.checkInterval))
        {
            return;
        }
        IReadOnlyList<Pawn> readOnlyList = (!Props.onlyPawnsInSameFaction || wearer.Faction is null) ? wearer.Map.mapPawns.AllPawnsSpawned : wearer.Map.mapPawns.SpawnedPawnsInFaction(wearer.Faction);
        foreach (Pawn pawn in readOnlyList)
        {
            if (!ValidPawn(pawn, wearer))
            {
                continue;
            }
            Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(Props.hediff);
            if (hediff is null)
            {
                hediff = pawn.health.AddHediff(Props.hediff, pawn.health.hediffSet.GetBrain());
                hediff.Severity = Props.initialSeverity;
                HediffComp_Link hediffComp_Link = hediff.TryGetComp<HediffComp_Link>();
                if (hediffComp_Link is not null)
                {
                    hediffComp_Link.drawConnection = true;
                    hediffComp_Link.other = wearer;
                }
            }
            HediffComp_Disappears hediffComp_Disappears = hediff.TryGetComp<HediffComp_Disappears>();
            if (hediffComp_Disappears is null)
            {
                Log.Error("HediffComp_GiveHediffsInRange has a hediff in props which does not have a HediffComp_Disappears");
            }
            else
            {
                hediffComp_Disappears.ticksToDisappear = Props.hediffDisappearTick;
            }
        }
    }

    private bool ValidPawn(Pawn pawn, Pawn wearer)
    {
        if (pawn.Dead || pawn.health is null)
        {
            return false;
        }
        if (!pawn.RaceProps.Humanlike)
        {
            return false;
        }
        if (pawn == wearer)
        {
            return Props.canApplyOnSelf;
        }
        if (pawn.Position.DistanceTo(wearer.Position) > Props.range)
        {
            return false;
        }
        if (!Props.targetingParameters.CanTarget(pawn))
        {
            return false;
        }
        return true;
    }
}
