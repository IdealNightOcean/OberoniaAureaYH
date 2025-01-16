using RimWorld;
using System.Collections.Generic;
using Verse;

namespace OberoniaAurea;

public class QuestPart_AllPawnHealthy : QuestPartActivable
{
    public int checkInterval = 2500;

    public string outSignalSuccess;
    public string outSignalFailed;

    public List<Pawn> pawns = [];

    public bool anyUnhealthyCauseFailure;
    public bool allHealthyCauseSuccess;

    private int ticksRemaining = 2500;

    public virtual bool AllPawnHealthy
    {
        get
        {
            if (pawns.NullOrEmpty())
            {
                return true;
            }
            foreach (Pawn p in pawns)
            {
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
        base.QuestPartTick();

        ticksRemaining--;
        if (ticksRemaining <= 0)
        {
            ticksRemaining = checkInterval;
            if (AllPawnHealthy)
            {
                if (allHealthyCauseSuccess)
                {
                    Complete();
                    Find.SignalManager.SendSignal(new Signal(outSignalSuccess));
                }
            }
            else if (anyUnhealthyCauseFailure)
            {
                Complete();
                Find.SignalManager.SendSignal(new Signal(outSignalFailed));
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
        if (pawnHediffSet == null) //没有健康状态属性那肯定是健康的（确信）
        {
            return true;
        }
        if (pawnHediffSet.BleedRateTotal > 0.001f)
        {
            return false;
        }
        if (pawnHediffSet.HasHediff(OARatkin_PawnInfoDefOf.OA_RK_SeriousInjury))
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
        pawns.Clear();
    }
    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref outSignalSuccess, "outSignalSuccess");
        Scribe_Values.Look(ref outSignalFailed, "outSignalFailed");
        Scribe_Values.Look(ref anyUnhealthyCauseFailure, "anyUnhealthyCauseFailure", defaultValue: false);
        Scribe_Values.Look(ref allHealthyCauseSuccess, "allHealthyCauseSuccess", defaultValue: false);
        Scribe_Values.Look(ref checkInterval, "checkInterval", 2500);
        Scribe_Values.Look(ref ticksRemaining, "ticksRemaining", 2500);
        Scribe_Collections.Look(ref pawns, "pawns", LookMode.Reference);

        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            pawns.RemoveAll((Pawn x) => x == null);
        }
    }
}

