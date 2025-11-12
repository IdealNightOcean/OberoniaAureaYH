using RimWorld;
using System.Collections.Generic;
using Verse;

namespace OberoniaAurea;

public class QuestPart_WoundedTravelerCanLeaveNow : QuestPartActivable
{
    private const int HealthCheckInterval = 30000;
    public string outSignal;

    public List<Pawn> pawns;

    private int ticksRemaining = 2500;

    public bool CanLeaveNow
    {
        get
        {
            if (pawns.NullOrEmpty())
            {
                return false;
            }
            for (int i = 0; i < pawns.Count; i++)
            {
                Pawn p = pawns[i];
                if (p.Map?.weatherManager.curWeather?.favorability == Favorability.VeryBad)
                {
                    return false;
                }
                if (!HealthyPawn(p))
                {
                    return false;
                }
            }
            return true;
        }
    }

    public override void QuestPartTick()
    {
        if ((--ticksRemaining) <= 0)
        {
            ticksRemaining = HealthCheckInterval;
            if (CanLeaveNow)
            {
                Find.SignalManager.SendSignal(new Signal(outSignal));
                Complete();
            }
        }
    }

    public static bool HealthyPawn(Pawn pawn) //判断一个Pawn是否健康
    {
        if (pawn.Destroyed || pawn.InMentalState)
        {
            return false;
        }
        HediffSet pawnHediffSet = pawn.health.hediffSet;
        if (pawnHediffSet is null) //没有健康状态属性那肯定是健康的（确信）
        {
            return true;
        }
        if (pawnHediffSet.BleedRateTotal > 0.001f)
        {
            return false;
        }
        if (pawnHediffSet.HasHediff(OARK_HediffDefOf.OARK_Hediff_SeriousInjury))
        {
            return false;
        }
        if (pawnHediffSet.HasNaturallyHealingInjury())
        {
            return false;
        }
        return true;
    }

    public override void Cleanup()
    {
        base.Cleanup();
        outSignal = null;

        pawns = null;
    }

    public override void ExposeData()
    {
        base.ExposeData();

        Scribe_Values.Look(ref ticksRemaining, "ticksRemaining", 2500);
        Scribe_Values.Look(ref outSignal, "outSignal");
        Scribe_Collections.Look(ref pawns, "pawns", LookMode.Reference);

        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            pawns?.RemoveAll(p => p is null);
        }
    }
}